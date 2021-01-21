using ArkhamOverlay.Data;
using ArkhamOverlay.Pages.ChooseEncounters;
using ArkhamOverlay.Pages.Overlay;
using ArkhamOverlay.Pages.SelectCards;
using ArkhamOverlay.Services;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using PageController;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
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

        private void ShowSelectCardsWindow(ISelectableCards selectableCards, string startingPositionConfigName) {
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

            var startingPositionProperty = ViewModel.AppData.Configuration.GetType().GetProperty(startingPositionConfigName);
            var startingPosition = (Point)startingPositionProperty.GetValue(ViewModel.AppData.Configuration);

            if (controller == null) {
                controller = _controllerFactory.CreateController<SelectCardsController>();
                controller.AppData = ViewModel.AppData;
                controller.SelectableCards = selectableCards;
                controller.Left = startingPosition.X == 0 ? left : startingPosition.X;
                controller.Top = startingPosition.Y == 0 ? top : startingPosition.Y;
                controller.Width = width;

                controller.Closed += () => {
                    _selectCardsControllers.Remove(controller);
                };

                controller.View.LocationChanged += (s, e) => {
                    startingPositionProperty.SetValue(ViewModel.AppData.Configuration, new Point(controller.View.Left, controller.View.Top));
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
        private void MainWindowActivated(object sender, EventArgs e) {
            foreach (var selectCardsWindow in _selectCardsControllers) {
                selectCardsWindow.Show();
            }
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
        public void SelectSnapshotDirectory() {
            var dialog = new CommonOpenFileDialog {
                InitialDirectory = ViewModel.Game.SnapshotDirectory,
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                ViewModel.Game.SnapshotDirectory = dialog.FileName;
            }
        }

        [Command]
        public void ShowOtherEncounters() {
            ShowSelectCardsWindow(ViewModel.AppData.Game.ScenarioCards, nameof(Configuration.ScenarioCardsPosition));
        }

        [Command]
        public void ShowLocations() {
            ShowSelectCardsWindow(ViewModel.AppData.Game.LocationCards, nameof(Configuration.LocationsPosition));
        }

        [Command]
        public void ShowEncounterDeck() {
            ShowSelectCardsWindow(ViewModel.AppData.Game.EncounterDeckCards, nameof(Configuration.EncounterCardsPosition));
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

        [Command]
        public void PlayerSelected(SelectableCards selectableCards) {
            var startingPositionProperty = string.Empty;
            if (ViewModel.Game.Players[0].SelectableCards == selectableCards) { startingPositionProperty = nameof(Configuration.Player1Position); }
            else if (ViewModel.Game.Players[1].SelectableCards == selectableCards) { startingPositionProperty = nameof(Configuration.Player2Position); }
            else if (ViewModel.Game.Players[2].SelectableCards == selectableCards) { startingPositionProperty = nameof(Configuration.Player3Position); }
            else if (ViewModel.Game.Players[3].SelectableCards == selectableCards) { startingPositionProperty = nameof(Configuration.Player4Position); }

            ShowSelectCardsWindow(selectableCards, startingPositionProperty);
        }

        [Command]
        public void ShowDeckList(SelectableCards selectableCards) {
            selectableCards.ShowDeckList();
        }

        [Command]
        public void ShowOverlay() {
            if (_overlayController != null) {
                _overlayController.Activate();
                return;
            }

            _overlayController = _controllerFactory.CreateController<OverlayController>();
            var overlayPosition = ViewModel.AppData.Configuration.OverlayPosition;

            _overlayController.Top = overlayPosition.Y == 0 ? View.Top + View.Height + 10 : overlayPosition.Y;
            if (overlayPosition.X != 0) {
                _overlayController.Left = overlayPosition.X;
            }

            _overlayController.Closed += () => {
                _overlayController = null;
                ViewModel.OverlayDisplayed = false;
            };

            _overlayController.View.LocationChanged += (s, e) => {
                ViewModel.AppData.Configuration.OverlayPosition = new Point(_overlayController.Left, _overlayController.Top);
            };

            _overlayController.Show();
            ViewModel.OverlayDisplayed = true;
        }

        [Command]
        public void ClearCards() {
            ViewModel.AppData.Game.ClearAllCards();
        }

        [Command]
        public void ShowAllWindows() {
            if (_overlayController != null) {
                _overlayController.Activate();
            }
            foreach (var selectCardsWindow in _selectCardsControllers) {
                selectCardsWindow.Activate();
            }
        }
        
        [Command]
        public void ResetOverlayColor() {
            ViewModel.Configuration.OverlayColor = ConfigurationService.DefaultBackgroundColor;
        }

        [Command]
        public void TakeSnapshot() {
            _overlayController.TakeSnapshot();
        }
    }
}
