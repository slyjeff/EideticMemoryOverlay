using ArkhamOverlay.Data;
using PageController;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArkhamOverlay.Pages.ChooseEncounters {
    public class ChooseEncountersController : Controller<ChooseEncountersView, ChooseEncountersViewModel> {
        private readonly AppData _appData;
        private readonly IList<SelectableEncounterSet> _selectableEncounterSets = new List<SelectableEncounterSet>();

        public ChooseEncountersController(AppData appData) {
            _appData = appData;
            foreach (var pack in appData.Configuration.Packs) {
                var cycle = GetCyle(pack);

                foreach (var encounterSet in pack.EncounterSets) {
                    var selectableEncounterSet = new SelectableEncounterSet(encounterSet);

                    cycle.EncounterSets.Add(selectableEncounterSet);
                    _selectableEncounterSets.Add(selectableEncounterSet);
                }
            }

            foreach (var encounterSet in appData.Game.EncounterSets) {
                var selectableEncounterSet = _selectableEncounterSets.FirstOrDefault(x => x.EncounterSet.Code == encounterSet.Code);
                if (selectableEncounterSet != null) {
                    selectableEncounterSet.IsSelected = true;
                }
            }
        }

        private SelectableEncounterCycle GetCyle(Pack pack) {
            while (ViewModel.Cycles.Count < pack.CyclePosition) {
                ViewModel.Cycles.Add(new SelectableEncounterCycle());
            }
            return ViewModel.Cycles[pack.CyclePosition - 1];
        }

        internal void ShowDialog() {
            View.ShowDialog();
        }


        [Command]
        public void Ok() {
            var encounterSets = new List<EncounterSet>();
            foreach (var encounterSet in _selectableEncounterSets) {
                if (encounterSet.IsSelected) {
                    encounterSets.Add(encounterSet.EncounterSet);
                }
            }
            _appData.Game.EncounterSets = encounterSets;
            _appData.Game.OnEncounterSetsChanged();
            View.Close();
        }

        [Command]
        public void Cancel() {
            View.Close();
        }
    }
}
