using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.Interfaces;
using EideticMemoryOverlay.PluginApi.LocalCards;
using Emo.Data;
using Emo.Services;
using PageController;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emo.Pages.ChooseEncounters {
    public class ChooseEncountersController<T> : Controller<ChooseEncountersView, ChooseEncountersViewModel>, IDisplayableView where T : LocalCard {
        private readonly IGameData _gameData;
        private readonly IPlugIn _plugIn;
        private readonly LoggingService _logger;
        private readonly IList<SelectableEncounterSet> _selectableEncounterSets = new List<SelectableEncounterSet>();
        private readonly IList<SelectableLocalPackManifest<T>> _selectableLocalPackManifests = new List<SelectableLocalPackManifest<T>>();

        public ChooseEncountersController(IGameData gameData, IPlugIn plugIn, LoggingService loggingService, ILocalCardsService<T> localCardsService) {
            _gameData = gameData;
            _plugIn = plugIn;
            _logger = loggingService;
            foreach (var pack in plugIn.Packs) {
                var cycle = GetCyle(pack);

                foreach (var encounterSet in pack.EncounterSets) {
                    var selectableEncounterSet = new SelectableEncounterSet(encounterSet) {
                        IsSelected = _gameData.EncounterSets.Any(x => x.Code == encounterSet.Code)
                    };

                    cycle.EncounterSets.Add(selectableEncounterSet);
                    _selectableEncounterSets.Add(selectableEncounterSet);
                }
            }

            var manifests = localCardsService.GetLocalPackManifests();
            if (!manifests.Any()) {
                return;
            }

            var localCycle = new SelectableEncounterCycle();
            ViewModel.Cycles.Add(localCycle);
            foreach (var manifest in localCardsService.GetLocalPackManifests()) {
                var selectableLocalPackManifest = new SelectableLocalPackManifest<T>(manifest) {
                    IsSelected = _gameData.LocalPacks.Any(x => string.Equals(x, manifest.Name, StringComparison.InvariantCulture))
                };

                _selectableLocalPackManifests.Add(selectableLocalPackManifest);
                localCycle.EncounterSets.Add(selectableLocalPackManifest);
            }
        }

        private SelectableEncounterCycle GetCyle(Pack pack) {
            while (ViewModel.Cycles.Count < pack.CyclePosition) {
                ViewModel.Cycles.Add(new SelectableEncounterCycle());
            }
            return ViewModel.Cycles[pack.CyclePosition - 1];
        }

        public void ShowView() {
            _logger.LogMessage("Showing encounter selection dialog.");
            View.ShowDialog();
        }


        [Command]
        public void Ok() {
            _logger.LogMessage("User confirmed encounter selection.");
            var encounterSets = new List<EncounterSet>();
            foreach (var encounterSet in _selectableEncounterSets) {
                if (encounterSet.IsSelected) {
                    encounterSets.Add(encounterSet.EncounterSet);
                }
            }
            _gameData.EncounterSets = encounterSets;

            var localPacks = new List<string>();
            foreach (var localPackManifest in _selectableLocalPackManifests) {
                if (localPackManifest.IsSelected) {
                    localPacks.Add(localPackManifest.Manifest.Name);
                }
            }
            _gameData.LocalPacks = localPacks;
            _plugIn.LoadEncounterCards();

            View.Close();
        }

        [Command]
        public void Cancel() {
            _logger.LogMessage("User cancelled encounter selection.");
            View.Close();
        }
    }
}
