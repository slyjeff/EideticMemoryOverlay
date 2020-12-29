using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using ArkhamOverlay.Services;
using System.Windows.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using System.Linq;
using ArkhamOverlay.Data;

namespace ArkhamOverlay {
    public partial class Main : Window {
        private Overlay _overlay;
        private readonly ArkhamDbService _arkhamDbService = new ArkhamDbService();
        private readonly IList<SelectCards> _selectCardsList = new List<SelectCards>();

        public Main() {
            InitializeComponent();
            DataContext = new AppData();
        }

        public void InitializeApp(object sender, RoutedEventArgs e) {
            LoadConfiguration();
            LoadLastSavedGame();
        }

        private void LoadConfiguration() {
            var configuration = new Configuration {
                OverlayWidth = 1228,
                OverlayHeight = 720,
                CardHeight = 300
            };

            if (File.Exists("Config.json")) {
                try {
                    configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("Config.json"));
                } catch {
                    // if there's an error, we don't care- just use the default configuration
                }
            }

            configuration.OverlayConfigurationChanged += () => {
                File.WriteAllText("Config.json", JsonConvert.SerializeObject(AppData.Configuration));
            };

            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                if (_arkhamDbService.FindMissingEncounterSets(configuration)) {
                    File.WriteAllText("Config.json", JsonConvert.SerializeObject(configuration));
                }
            };
            worker.RunWorkerAsync();

            AppData.Configuration = configuration;
        }

        private void LoadLastSavedGame() {
            var game = new Game();
            if (File.Exists("LastSaved.json")) {
                try {
                    game = JsonConvert.DeserializeObject<Game>(File.ReadAllText("LastSaved.json"));
                } catch {
                    // if there's an error, we don't care- just use the default game
                }
            }

            while (game.Players.Count < 4) {
                game.Players.Add(new Player());
            }

            SetGame(game);
        }

        public void CloseApp(object sender, RoutedEventArgs e) {
            ClearPlayerCardsList();
            if (_overlay != null) {
                _overlay.Close();
            }
        }

        private void ClearPlayerCardsList() {
            while (_selectCardsList.Count > 0) {
                _selectCardsList.First().Close();
            }
        }

        public AppData AppData { get { return DataContext as AppData; } }

        public void SaveGame(object sender, RoutedEventArgs e) {
            File.WriteAllText(AppData.Game.Name + ".json", JsonConvert.SerializeObject(AppData.Game));
            File.WriteAllText("LastSaved.json", JsonConvert.SerializeObject(AppData.Game));
        }

        public void LoadGame(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog {
                DefaultExt = "json",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (dialog.ShowDialog() == true) {
                var game = JsonConvert.DeserializeObject<Game>(File.ReadAllText(dialog.FileName));
                SetGame(game);
            }
        }

        private void SetGame(Game game) {
            ClearPlayerCardsList();

            _arkhamDbService.LoadAllPlayers(game);

            AppData.Game = game;
            AppData.OnGameChanged();
            LoadEncounterCards();

            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                foreach (var player in game.Players) {
                    _arkhamDbService.LoadPlayerCards(player);
                }
            };
            worker.RunWorkerAsync();
        }

        public void SetEncounterSet(object sender, RoutedEventArgs e) {
            var chooseEncounters = new ChooseEncounters();
            chooseEncounters.SetAppData(AppData);
            chooseEncounters.ShowDialog();

            LoadEncounterCards();
        }

        private void LoadEncounterCards() {
            EncounterCardOptions.Visibility = AppData.Game.EncounterSets.Any() ? Visibility.Visible : Visibility.Collapsed;

            var appData = AppData;
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                _arkhamDbService.LoadEncounterCards(appData);
            };
            worker.RunWorkerAsync();
        }

        public void ShowOtherEncounters(object sender, RoutedEventArgs e) {
            ShowSelectCardsWindow(AppData.Game.ScenarioCards);

        }

        public void ShowLocations(object sender, RoutedEventArgs e) {
            ShowSelectCardsWindow(AppData.Game.LocationCards);
        }

        public void ShowEncounterDeck(object sender, RoutedEventArgs e) {
            ShowSelectCardsWindow(AppData.Game.EncounterDeckCards);
        }

        public void Refresh(object sender, RoutedEventArgs e) {
            if (!(sender is Button button)) {
                return;
            }

            if (!(button.DataContext is Player player)) {
                return;
            }

            _arkhamDbService.LoadPlayer(player);
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                _arkhamDbService.LoadPlayerCards(player);
            };
            worker.RunWorkerAsync();
        }

        private void MainWindowActivated(object sender, EventArgs e) {
            foreach (var selectCards in _selectCardsList) {
                selectCards.Show();
            }
        }

        private void PlayerSelected(object sender, RoutedEventArgs e) {
            if (!(sender is Button button)) {
                return;
            }

            if (!(button.DataContext is Player player)) {
                return;
            }

            ShowSelectCardsWindow(player.SelectableCards);
        }

        private void PlayerSelected(object sender, MouseButtonEventArgs e) {
            if (!(sender is Image image)) {
                return;
            }

            if (!(image.DataContext is Player player)) {
                return;
            }

            ShowSelectCardsWindow(player.SelectableCards);
        }

        private void ShowSelectCardsWindow(SelectableCards selectableCard) {
            var left = Left + Width + 10;
            var width = (double)786;
            var top = Top;
            SelectCards selectCardsWindow = null;
            foreach (var selectCardsWindowInList in _selectCardsList) {
                if (selectCardsWindowInList.SelectableCards == selectableCard) {
                    selectCardsWindow = selectCardsWindowInList;
                    selectCardsWindow.Activate();
                    break;
                } else {
                    if (selectCardsWindowInList.Top + selectCardsWindowInList.Height > top) {
                        top = selectCardsWindowInList.Top + selectCardsWindowInList.Height + 10;
                        width = selectCardsWindowInList.Width;
                        left = selectCardsWindowInList.Left;
                    }
                }
            }

            if (selectCardsWindow == null) {
                selectCardsWindow = new SelectCards {
                    SelectableCards = selectableCard,
                    Overlay = _overlay,
                    Left = left,
                    Top = top,
                    Width = width
                };

                selectCardsWindow.Closed += (x, y) => {
                    _selectCardsList.Remove(selectCardsWindow);
                };

                _selectCardsList.Add(selectCardsWindow);
            }

            if (!selectCardsWindow.SelectableCards.Loading) {
                selectCardsWindow.Show();
            } else {
                ShowSelectCardsWindowWhenFinishedLoading(selectCardsWindow);
            }
        }

        private void ShowSelectCardsWindowWhenFinishedLoading(SelectCards selectCardsWindow) {
            var timer = new DispatcherTimer {
                Interval = new TimeSpan(500)
            };

            timer.Tick += (x, y) => {
                if (selectCardsWindow.SelectableCards.Loading) {
                    return;
                }

                timer.Stop();
                selectCardsWindow.Show();
            };

            timer.Start();
        }

        public void ShowOverlay(object sender, RoutedEventArgs e) {
            if (_overlay != null) {
                _overlay.Activate();
                return;
            }

            var overlayData = new OverlayData {
                Configuration = AppData.Configuration
            };

            _overlay = new Overlay {
                Top = Top + Height + 10,
                DataContext = overlayData
            };

            _overlay.Closed += (x, y) => {
                _overlay = null;
            };

            foreach (var playerCards in _selectCardsList) {
                playerCards.Overlay = _overlay;
            }

            _overlay.Show();
            ClearCardsButton.Visibility = Visibility.Visible;
        }

        public void ClearCards(object sender, RoutedEventArgs e) {
            if (_overlay == null) {
                return;
            }

            _overlay.ClearCards();
        }
    }
}