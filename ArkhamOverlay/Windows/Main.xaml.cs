using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using ArkhamOverlay.Services;
using System.Windows.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using System.Linq;
using ArkhamOverlay.Data;
using ArkhamOverlay.TcpUtils;

namespace ArkhamOverlay {
    public partial class Main : Window {
        private Overlay _overlay;
        private readonly ArkhamDbService _arkhamDbService = new ArkhamDbService();
        private readonly IList<SelectCards> _selectCardsWindows = new List<SelectCards>();

        private readonly AppData _appData = new AppData();
        private readonly ConfigurationService _configurationService;
        private readonly GameFileService _gameFileService;

        public Main() {
            InitializeComponent();
            _configurationService = new ConfigurationService(_appData);
            _gameFileService = new GameFileService(_appData);
            new CardLoadService(_appData);

            DataContext = _appData;
        }

        public void InitializeApp(object sender, RoutedEventArgs e) {
            _configurationService.Load();
            _gameFileService.Load("LastSaved.json");
            LoadEncounterSets();

            var socketService = new ReceiveSocketService(new TcpRequestHandler(_appData));
            socketService.StartListening(TcpInfo.ArkhamOverlayPort);
        }

        private void LoadEncounterSets() {
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                _arkhamDbService.FindMissingEncounterSets(_appData.Configuration);
            };
            worker.RunWorkerAsync();
        }

        public void CloseApp(object sender, RoutedEventArgs e) {
            ClearPlayerCardsWindows();

            if (_overlay != null) {
                _overlay.Close();
            }
        }

        private void ClearPlayerCardsWindows() {
            while (_selectCardsWindows.Count > 0) {
                _selectCardsWindows.First().Close();
            }
        }

        public void SaveGame(object sender, RoutedEventArgs e) {
            _gameFileService.Save(_appData.Game.Name + ".json");
        }

        public void LoadGame(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog {
                DefaultExt = "json",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (dialog.ShowDialog() == true) {
                _gameFileService.Load(dialog.FileName);
                ClearPlayerCardsWindows();
            }
        }

        public void SetEncounterSet(object sender, RoutedEventArgs e) {
            var chooseEncounters = new ChooseEncounters();
            chooseEncounters.SetAppData(_appData);
            chooseEncounters.ShowDialog();
        }

        public void ShowOtherEncounters(object sender, RoutedEventArgs e) {
            ShowSelectCardsWindow(_appData.Game.ScenarioCards);

        }

        public void ShowLocations(object sender, RoutedEventArgs e) {
            ShowSelectCardsWindow(_appData.Game.LocationCards);
        }

        public void ShowEncounterDeck(object sender, RoutedEventArgs e) {
            ShowSelectCardsWindow(_appData.Game.EncounterDeckCards);
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
            foreach (var selectCardsWindow in _selectCardsWindows) {
                selectCardsWindow.Show();
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
            foreach (var selectCardsWindowInList in _selectCardsWindows) {
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
                    Left = left,
                    Top = top,
                    Width = width
                };

                selectCardsWindow.Closed += (x, y) => {
                    _selectCardsWindows.Remove(selectCardsWindow);
                };

                _selectCardsWindows.Add(selectCardsWindow);
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


            _overlay = new Overlay {
                Top = Top + Height + 10,
            };
            _overlay.SetAppData(_appData);

            _overlay.Closed += (x, y) => {
                _overlay = null;
            };

            _overlay.Show();
        }

        public void ClearCards(object sender, RoutedEventArgs e) {
            _appData.Game.ClearAllCards();
        }

        public void ToggleActAgendaBar(object sender, RoutedEventArgs e) {
            if (_overlay != null) {
                _overlay.ToggleActAgendaBar();
            }
        }
    }
}