using ArkhamOverlay.Data;
using System.ComponentModel;

namespace ArkhamOverlay.Services {
    public class CardLoadService {
        private readonly AppData _appData;
        private readonly ArkhamDbService _arkhamDbService = new ArkhamDbService();

        public CardLoadService(ArkhamDbService arkhamDbService, AppData appData) {
            _arkhamDbService = arkhamDbService;
            _appData = appData;
        }

        internal void RegisterEvents() {
            _appData.Game.PlayersChanged += LoadPlayerCards;
            _appData.Game.EncounterSetsChanged += LoadEncounterCards;
        }

        private void LoadEncounterCards() {
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                _arkhamDbService.LoadEncounterCards(_appData);
            };
            worker.RunWorkerAsync();
        }

        private void LoadPlayerCards() {
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                foreach (var player in _appData.Game.Players) {
                    _arkhamDbService.LoadPlayerCards(player);
                }
            };
            worker.RunWorkerAsync();
        }
    }
}
