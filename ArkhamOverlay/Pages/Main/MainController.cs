using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Events;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Data;
using ArkhamOverlay.Pages.ChooseEncounters;
using ArkhamOverlay.Pages.LocalImages;
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
        private readonly ArkhamDbService _arkhamDbService;
        private readonly IList<SelectCardsController> _selectCardsControllers = new List<SelectCardsController>();

        private readonly GameFileService _gameFileService;
        private readonly IControllerFactory _controllerFactory;
        private readonly LoadingStatusService _loadingStatusService;
        private readonly LoggingService _logger;
        private readonly IEventBus _eventBus;

        public MainController(AppData appData, GameFileService gameFileService, IControllerFactory controllerFactory, ArkhamDbService arkhamDbService, LoadingStatusService loadingStatusService, LoggingService loggingService, IEventBus eventBus) {
            ViewModel.AppData = appData;

            _gameFileService = gameFileService;
            _controllerFactory = controllerFactory;
            _arkhamDbService = arkhamDbService;
            _loadingStatusService = loadingStatusService;
            _logger = loggingService;
            _eventBus = eventBus;

            LoadEncounterSets();

            View.Closed += (s, e) => {
                _logger.LogMessage("Closing main window.");
                ClearPlayerCardsWindows();

                if (_overlayController != null) {
                    _overlayController.Close();
                }
            };
        }

        private void LoadEncounterSets() {
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                _logger.LogMessage("Main window loading encounter sets.");
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
            _logger.LogMessage($"Showing select card window: {selectableCards.Name}.");
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
                _logger.LogMessage($"Delayed showing select card window: {selectableCards.Name}.");
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

                _logger.LogMessage($"Showing select card window after delay: {selectCardsController.SelectableCards.Name}.");
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
            var fileName = ViewModel.AppData.Game.Name + ".json";
            _logger.LogMessage($"Main window: saving game to {fileName}.");
            _gameFileService.Save(fileName);
        }

        [Command]
        public void LoadGame() {
            _logger.LogMessage("Main window: load game clicked.");
            var dialog = new OpenFileDialog {
                DefaultExt = "json",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (dialog.ShowDialog() == true) {
                _logger.LogMessage($"Main window: loading game from {dialog.FileName}.");
                _gameFileService.Load(dialog.FileName);
                ClearPlayerCardsWindows();
            }
        }

        [Command]
        public void LocalImages() {
            _logger.LogMessage("Main window: manage local images clicked.");
            var controller = _controllerFactory.CreateController<LocalImagesController>();
            controller.ShowView();
            //reload

        }

        [Command]
        public void SetEncounterSets() {
            _logger.LogMessage("Main window: set encounter sets clicked.");
            var chooseEncounters = _controllerFactory.CreateController<ChooseEncountersController>();
            chooseEncounters.ShowDialog();
        }

        [Command]
        public void SelectSnapshotDirectory() {
            _logger.LogMessage("Main window: select snapshot directory clicked.");
            var dialog = new CommonOpenFileDialog {
                InitialDirectory = ViewModel.Game.SnapshotDirectory,
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                _logger.LogMessage($"Main window: new snapshot directory: {dialog.FileName}.");
                ViewModel.Game.SnapshotDirectory = dialog.FileName;
            }
        }

        [Command]
        public void ShowOtherEncounters() {
            _logger.LogMessage("Main window: show scenario cards clicked.");
            ShowSelectCardsWindow(ViewModel.AppData.Game.ScenarioCards, nameof(Configuration.ScenarioCardsPosition));
        }

        [Command]
        public void ShowLocations() {
            _logger.LogMessage("Main window: location cards clicked.");
            ShowSelectCardsWindow(ViewModel.AppData.Game.LocationCards, nameof(Configuration.LocationsPosition));
        }

        [Command]
        public void ShowEncounterDeck() {
            _logger.LogMessage("Main window: show encounter cards clicked.");
            ShowSelectCardsWindow(ViewModel.AppData.Game.EncounterDeckCards, nameof(Configuration.EncounterCardsPosition));
        }

        [Command]
        public void Refresh(Player player) {
            _logger.LogMessage($"Main window: refresh player {player.ID} cards clicked.");
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
                catch (Exception ex) {
                    _logger.LogException(ex, $"Main window: error refreshing player {player.ID} cards.");
                    _loadingStatusService.ReportPlayerStatus(player.ID, Status.Error);
                }
            }
        }

        [Command]
        public void PlayerSelected(SelectableCards selectableCards) {
            _logger.LogMessage($"Main window: player selected: {selectableCards.Name}.");
            var startingPositionProperty = string.Empty;
            if (ViewModel.Game.Players[0].SelectableCards == selectableCards) { startingPositionProperty = nameof(Configuration.Player1Position); }
            else if (ViewModel.Game.Players[1].SelectableCards == selectableCards) { startingPositionProperty = nameof(Configuration.Player2Position); }
            else if (ViewModel.Game.Players[2].SelectableCards == selectableCards) { startingPositionProperty = nameof(Configuration.Player3Position); }
            else if (ViewModel.Game.Players[3].SelectableCards == selectableCards) { startingPositionProperty = nameof(Configuration.Player4Position); }

            ShowSelectCardsWindow(selectableCards, startingPositionProperty);
        }

        [Command]
        public void ShowDeckList(Deck deck) {
            _logger.LogMessage("Main window: show deck list clicked.");
            _eventBus.PublishShowDeckListRequest(deck);
        }

        [Command]
        public void ShowOverlay() {
            _logger.LogMessage("Main window: show overlay clicked.");
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
            _logger.LogMessage("Main window: clear all cards clicked.");
            ViewModel.AppData.Game.ClearAllCards();
        }

        [Command]
        public void ShowAllWindows() {
            _logger.LogMessage("Main window: show all windows clicked.");
            if (_overlayController != null) {
                _overlayController.Activate();
            }
            foreach (var selectCardsWindow in _selectCardsControllers) {
                selectCardsWindow.Activate();
            }
        }

        [Command]
        public void SelectAutoSnapshotFile() {
            _logger.LogMessage("Main window: select auto snapshot file clicked.");
            var dialog = new CommonOpenFileDialog {
                InitialDirectory = ViewModel.Configuration.AutoSnapshotFilePath,
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                ViewModel.Configuration.AutoSnapshotFilePath = dialog.FileName;
            }
        }

        [Command]
        public void ResetOverlayColor() {
            _logger.LogMessage("Main window: reset overlay color clicked.");
            ViewModel.Configuration.OverlayColor = ConfigurationService.DefaultBackgroundColor;
        }

        [Command]
        public void TakeSnapshot() {
            _logger.LogMessage("Main window: take snapshot clicked.");
            _eventBus.PublishTakeSnapshotRequest();
        }
    }
}
