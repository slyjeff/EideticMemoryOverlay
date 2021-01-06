using ArkhamOverlay.Data;
using System.ComponentModel;

namespace ArkhamOverlay.Services {
    public class CardLoadService {
        private readonly AppData _appData;
        private readonly LoadingStatusService _loadingStatusService;
        private readonly ArkhamDbService _arkhamDbService;

        public CardLoadService(ArkhamDbService arkhamDbService, AppData appData, LoadingStatusService loadingStatusService) {
            _arkhamDbService = arkhamDbService;
            _appData = appData;
            _loadingStatusService = loadingStatusService;
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
                catch {
                    _loadingStatusService.ReportEncounterCardsStatus(Status.Error);
                }

                _arkhamDbService.LoadEncounterCards(_appData);
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
                        catch {
                            _loadingStatusService.ReportPlayerStatus(player.ID, Status.Error);
                        }
                    }
                }
            };
            worker.RunWorkerAsync();
        }
    }
}
