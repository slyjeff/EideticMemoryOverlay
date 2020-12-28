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

namespace ArkhamOverlay {
    public partial class Main : Window {
        private Overlay _overlay;
        private readonly ArkhamDbService _arkhamDbService = new ArkhamDbService();
        private readonly IList<PlayerCards> _playerCardsList = new List<PlayerCards>();

        public Main() {
            InitializeComponent();
            DataContext = new AppData();
        }

        public void InitializeApp(object sender, RoutedEventArgs e) {
            AppData.Configuration = new Configuration {
                OverlayWidth = 1228,
                OverlayHeight = 720,
                CardHeight = 300
            };

            if (File.Exists("Config.json")) {
                try {
                    AppData.Configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("Config.json"));
                } catch {
                    // if there's an error, we don't care- just use the default configuration
                }
            } 

            AppData.Configuration.OverlayConfigurationChanged += () => {
                File.WriteAllText("Config.json", JsonConvert.SerializeObject(AppData.Configuration));
            };

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
            while (_playerCardsList.Count > 0) {
                _playerCardsList.First().Close();
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

            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                foreach (var player in game.Players) {
                    _arkhamDbService.LoadPlayerCards(player);
                }
            };
            worker.RunWorkerAsync();
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
            foreach (var playerCards in _playerCardsList) {
                playerCards.Show();
            }
        }

        private void PlayerSelected(object sender, MouseButtonEventArgs e) {
            if (!(sender is Image image)) {
                return;
            }

            if (!(image.DataContext is Player player)) {
                return;
            }

            var left = Left + Width + 10;
            var width = (double)786;
            var top = Top;
            PlayerCards playerCards = null;
            foreach (var playerCardsInList in _playerCardsList) {
                if (playerCardsInList.Player == player) {
                    playerCards = playerCardsInList;
                    playerCards.Activate();
                    break;
                } else {
                    if (playerCardsInList.Top + playerCardsInList.Height > top) {
                        top = playerCardsInList.Top + playerCardsInList.Height + 10;
                        width = playerCardsInList.Width;
                        left = playerCardsInList.Left;
                    }
                }
            }

            if (playerCards == null) {
                playerCards = new PlayerCards {
                    Player = player,
                    Overlay = _overlay,
                    Left = left,
                    Top = top,
                    Width = width
                };

                playerCards.Closed += (x, y) => {
                    _playerCardsList.Remove(playerCards);
                };

                _playerCardsList.Add(playerCards);
            }

            if (!playerCards.Player.Loading) {
                playerCards.Show();
            } else {
                ShowPlayerCardsWhenFinishedLoading(playerCards);
            }
        }

        private void ShowPlayerCardsWhenFinishedLoading(PlayerCards playerCards) {
            var timer = new DispatcherTimer {
                Interval = new TimeSpan(500)
            };

            timer.Tick += (x, y) => {
                if (playerCards.Player.Loading) {
                    return;
                }

                timer.Stop();
                playerCards.Show();
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

            foreach (var playerCards in _playerCardsList) {
                playerCards.Overlay = _overlay;
            }

            _overlay.Show();
        }
    }
}