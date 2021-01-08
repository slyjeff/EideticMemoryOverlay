using ArkhamOverlay.Data;
using ArkhamOverlay.Pages.ChooseEncounters;
using ArkhamOverlay.Pages.Overlay;
using ArkhamOverlay.Pages.SelectCards;
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
        private OverlayController _overlayController;
        private readonly ArkhamDbService _arkhamDbService = new ArkhamDbService();
        private readonly IList<SelectCardsController> _selectCardsControllers = new List<SelectCardsController>();

        private readonly GameFileService _gameFileService;
        private readonly IControllerFactory _controllerFactory;
        private readonly LoadingStatusService _loadingStatusService;

        public MainController(AppData appData, GameFileService gameFileService, IControllerFactory controllerFactory, LoadingStatusService loadingStatusService) {
            ViewModel.AppData = appData;

            _gameFileService = gameFileService;
            _controllerFactory = controllerFactory;
            _loadingStatusService = loadingStatusService;

            LoadEncounterSets();

            View.Closed += (s, e) => {
                ClearPlayerCardsWindows();

                if (_overlayController != null) {
                    _overlayController.Close();
                }
            };
        }

        private void LoadEncounterSets() {
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                _loadingStatusService.ReportEncounterCardsStatus(Status.LoadingDeck);
                _arkhamDbService.FindMissingEncounterSets(ViewModel.AppData.Configuration);
                _loadingStatusService.ReportEncounterCardsStatus(Status.Finished);
            };
            worker.RunWorkerAsync();
        }

        private void ClearPlayerCardsWindows() {
            while (_selectCardsControllers.Count > 0) {
                _selectCardsControllers.First().Close();
            }
        }

        private void ShowSelectCardsWindow(ISelectableCards selectableCards) {
            var left = View.Left + View.Width + 10;
            var width = (double)836;
            var top = View.Top;
            SelectCardsController controller = null;
            foreach (var selectCardsControllerInList in _selectCardsControllers) {
                if (selectCardsControllerInList.SelectableCards == selectableCards) {
                    controller = selectCardsControllerInList;
                    controller.Activate();
                    break;
                } else {
                    if (selectCardsControllerInList.Top + selectCardsControllerInList.Height > top) {
                        top = selectCardsControllerInList.Top + selectCardsControllerInList.Height + 10;
                        width = selectCardsControllerInList.Width;
                        left = selectCardsControllerInList.Left;
                    }
                }
            }

            if (controller == null) {
                controller = _controllerFactory.CreateController<SelectCardsController>();
                controller.SelectableCards = selectableCards;
                controller.Left = left;
                controller.Top = top;
                controller.Width = width;

                controller.Closed += () => {
                    _selectCardsControllers.Remove(controller);
                };

                _selectCardsControllers.Add(controller);
            }

            if (!controller.SelectableCards.Loading) {
                controller.Show();
            } else {
                ShowSelectCardsWindowWhenFinishedLoading(controller);
            }
        }

        private void ShowSelectCardsWindowWhenFinishedLoading(SelectCardsController selectCardsController) {
            var timer = new DispatcherTimer {
                Interval = new TimeSpan(500)
            };

            timer.Tick += (x, y) => {
                if (selectCardsController.SelectableCards.Loading) {
                    return;
                }

                timer.Stop();
                selectCardsController.Show();
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
            var chooseEncounters = _controllerFactory.CreateController<ChooseEncountersController>();
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
            if (!string.IsNullOrEmpty(player.DeckId)) {
                try {
                    _loadingStatusService.ReportPlayerStatus(player.ID, Status.LoadingDeck);
                    _arkhamDbService.LoadPlayer(player);

                    var worker = new BackgroundWorker();
                    worker.DoWork += (x, y) => {
                        _loadingStatusService.ReportPlayerStatus(player.ID, Status.LoadingCards);
                        try {
                            _arkhamDbService.LoadPlayerCards(player);
                            _loadingStatusService.ReportPlayerStatus(player.ID, Status.Finished);
                        }
                        catch {
                            _loadingStatusService.ReportPlayerStatus(player.ID, Status.Error);
                        }
                    };
                    worker.RunWorkerAsync();
                }
                catch {
                    _loadingStatusService.ReportPlayerStatus(player.ID, Status.Error);
                }
            }
        }

        private void MainWindowActivated(object sender, EventArgs e) {
            foreach (var selectCardsWindow in _selectCardsControllers) {
                selectCardsWindow.Show();
            }
        }

        [Command]
        public void PlayerSelected(SelectableCards selectableCards) {
            ShowSelectCardsWindow(selectableCards);
        }

        [Command]
        public void ShowOverlay() {
            if (_overlayController != null) {
                _overlayController.Activate();
                return;
            }

            _overlayController = _controllerFactory.CreateController<OverlayController>();
            _overlayController.Top = View.Top + View.Height + 10;

            _overlayController.Closed += () => {
                _overlayController = null;
            };

            _overlayController.Show();
        }

        [Command]
        public void ClearCards() {
            ViewModel.AppData.Game.ClearAllCards();
        }
    }
}
