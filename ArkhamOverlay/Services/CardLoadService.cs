using ArkhamOverlay.Data;
using System;
using System.ComponentModel;

namespace ArkhamOverlay.Services {
    public class CardLoadService {
        private readonly AppData _appData;
        private readonly LoadingStatusService _loadingStatusService;
        private readonly ArkhamDbService _arkhamDbService;
        private readonly LoggingService _logger;

        public CardLoadService(ArkhamDbService arkhamDbService, AppData appData, LoadingStatusService loadingStatusService, LoggingService loggingService) {
            _arkhamDbService = arkhamDbService;
            _appData = appData;
            _loadingStatusService = loadingStatusService;
            _logger = loggingService;
        }

        internal void RegisterEvents() {
            _appData.Game.PlayersChanged += LoadPlayerCards;
            _appData.Game.EncounterSetsChanged += LoadEncounterCards;
        }

        private void LoadEncounterCards() {
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                _loadingStatusService.ReportEncounterCardsStatus(Status.LoadingCards);
                try {
                    _arkhamDbService.LoadEncounterCards(_appData);
                    _loadingStatusService.ReportEncounterCardsStatus(Status.Finished);
                }
                catch (Exception ex) {
                    _logger.LogException(ex, "Error loading encounter cards.");
                    _loadingStatusService.ReportEncounterCardsStatus(Status.Error);
                }
            };
            worker.RunWorkerAsync();
        }

        private void LoadPlayerCards() {
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                foreach (var player in _appData.Game.Players) {
                    if (!string.IsNullOrEmpty(player.DeckId)) {
                        _loadingStatusService.ReportPlayerStatus(player.ID, Status.LoadingCards);
                        try {
                            _arkhamDbService.LoadPlayerCards(player);
                            _loadingStatusService.ReportPlayerStatus(player.ID, Status.Finished);
                        }
                        catch (Exception ex) {
                            _logger.LogException(ex, $"Error loading player cards for player {player.ID}");
                            _loadingStatusService.ReportPlayerStatus(player.ID, Status.Error);
                        }
                    }
                }
            };
            worker.RunWorkerAsync();
        }
    }
}
