using ArkhamHorrorLcg.ArkhamDb;
using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace ArkhamHorrorLcg {
    internal interface IPackLoader {
        void FindMissingEncounterSets();
    }

    internal class PackLoader : IPackLoader {
        private readonly IArkhamConfiguration _configuration;
        private readonly ILoggingService _logger;
        private readonly IArkhamDbService _arkhamDbService;

        public PackLoader(IArkhamConfiguration arkhamConfiguration, ILoggingService logger, IArkhamDbService arkhamDbService) {
            _configuration = arkhamConfiguration;
            _logger = logger;
            _arkhamDbService = arkhamDbService;
        }

        public void FindMissingEncounterSets() {
            _logger.LogMessage("Looking for encounter sets.");

            var setsAdded = false;
            var packs = _arkhamDbService.GetAllPacks();

            foreach (var pack in packs) {
                if (!_configuration.Packs.Any(x => x.Code == pack.Code)) {
                    if (AddPackToConfiguration(pack)) {
                        setsAdded = true;
                    }
                }
            }

            _logger.LogMessage($"Found new encounter sets: {setsAdded}.");

            if (setsAdded) {
                _configuration.Save();
            }
        }

        private bool AddPackToConfiguration(ArkhamDbPack arkhamDbPack) {
            var cards = _arkhamDbService.GetCardsInPack(arkhamDbPack.Code);
            var encounterSets = new List<EncounterSet>();
            foreach (var card in cards) {
                if (string.IsNullOrEmpty(card.Encounter_Code) || encounterSets.Any(x => x.Code == card.Encounter_Code)) {
                    continue;
                }

                encounterSets.Add(new EncounterSet { Code = card.Encounter_Code, Name = card.Encounter_Name });
            }

            if (!encounterSets.Any()) {
                return false;
            }

            var newPack = new Pack {
                Code = arkhamDbPack.Code,
                Name = arkhamDbPack.Name,
                CyclePosition = arkhamDbPack.Cycle_Position,
                Position = arkhamDbPack.Position,
                EncounterSets = encounterSets
            };

            _configuration.Packs.Add(newPack);

            _logger.LogMessage($"Added pack {newPack.Name} to encounter sets.");

            return true;
        }
    }
}
