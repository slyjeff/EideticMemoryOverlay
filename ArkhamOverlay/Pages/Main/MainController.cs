using ArkhamOverlay.Data;
using ArkhamOverlay.Services;
using Microsoft.Win32;
using PageController;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;

namespace ArkhamOverlay.Pages.Main {
    public class MainController : Controller<MainView, MainViewModel> {
        private Overlay _overlay;
        private readonly ArkhamDbService _arkhamDbService = new ArkhamDbService();
        private readonly IList<SelectCards> _selectCardsWindows = new List<SelectCards>();

        private readonly GameFileService _gameFileService;

        public MainController(AppData appData, GameFileService gameFileService) {
            ViewModel.AppData = appData;

            _gameFileService = gameFileService;

            LoadEncounterSets();

            View.Closed += (s, e) => {
                ClearPlayerCardsWindows();

                if (_overlay != null) {
                    _overlay.Close();
                }
            };
        }

        private void LoadEncounterSets() {
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                _arkhamDbService.FindMissingEncounterSets(ViewModel.AppData.Configuration);
            };
            worker.RunWorkerAsync();
        }

        private void ClearPlayerCardsWindows() {
            while (_selectCardsWindows.Count > 0) {
                _selectCardsWindows.First().Close();
            }
        }

        private void ShowSelectCardsWindow(SelectableCards selectableCard) {
            var left = View.Left + View.Width + 10;
            var width = (double)786;
            var top = View.Top;
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


        [Command]
        public void SaveGame() {
            _gameFileService.Save(ViewModel.AppData.Game.Name + ".json");
        }

        [Command]
        public void LoadGame() {
            var dialog = new OpenFileDialog {
                DefaultExt = "json",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (dialog.ShowDialog() == true) {
                _gameFileService.Load(dialog.FileName);
                ClearPlayerCardsWindows();
            }
        }

        [Command]
        public void SetEncounterSets() {
            var chooseEncounters = new ChooseEncounters();
            chooseEncounters.SetAppData(ViewModel.AppData);
            chooseEncounters.ShowDialog();
        }

        [Command]
        public void ShowOtherEncounters() {
            ShowSelectCardsWindow(ViewModel.AppData.Game.ScenarioCards);
        }

        [Command]
        public void ShowLocations() {
            ShowSelectCardsWindow(ViewModel.AppData.Game.LocationCards);
        }

        [Command]
        public void ShowEncounterDeck() {
            ShowSelectCardsWindow(ViewModel.AppData.Game.EncounterDeckCards);
        }

        [Command]
        public void Refresh(Player player) {
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

        [Command]
        public void PlayerSelected(SelectableCards selectableCards) {
            ShowSelectCardsWindow(selectableCards);
        }

        [Command]
        public void ShowOverlay() {
            if (_overlay != null) {
                _overlay.Activate();
                return;
            }


            _overlay = new Overlay {
                Top = View.Top + View.Height + 10,
            };
            _overlay.SetAppData(ViewModel.AppData);

            _overlay.Closed += (x, y) => {
                _overlay = null;
            };

            _overlay.Show();
        }

        [Command]
        public void ClearCards(object sender) {
            ViewModel.AppData.Game.ClearAllCards();
        }

        [Command]
        public void ToggleActAgendaBar() {
            if (_overlay != null) {
                _overlay.ToggleActAgendaBar();
            }
        }
    }
}
