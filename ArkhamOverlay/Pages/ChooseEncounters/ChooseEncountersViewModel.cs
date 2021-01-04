using ArkhamOverlay.Data;
using PageController;
using System.Collections.Generic;

namespace ArkhamOverlay.Pages.ChooseEncounters {
    public class ChooseEncountersViewModel : ViewModel {
        public ChooseEncountersViewModel() {
            Cycles = new List<SelectableEncounterCycle>();
        }

        public virtual IList<SelectableEncounterCycle> Cycles { get; private set; }
    }

    public class SelectableEncounterCycle {
        public SelectableEncounterCycle() {
            EncounterSets = new List<SelectableEncounterSet>();
        }

        public IList<SelectableEncounterSet> EncounterSets { get; private set; }
    }

    public class SelectableEncounterSet {
        public SelectableEncounterSet(EncounterSet encounterSet) {
            EncounterSet = encounterSet;
        }

        public EncounterSet EncounterSet { get; private set; }
        public bool IsSelected { get; set; }
    }
}
