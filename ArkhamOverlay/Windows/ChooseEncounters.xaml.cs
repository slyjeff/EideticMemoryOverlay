using ArkhamOverlay.Data;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ArkhamOverlay {
    public partial class ChooseEncounters : Window {
        private AppData _appData;
        private readonly IList<SelectableEncounterSet> _selectableEncounterSets = new List<SelectableEncounterSet>();

        public ChooseEncounters() {
            InitializeComponent();
        }

        public void SetAppData(AppData appData) {
            _appData = appData;
            var cycles = new SelectableEncounterCycles();
            foreach (var pack in appData.Configuration.Packs) {
                var cycle = cycles.GetCyle(pack);

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

            DataContext = cycles;
        }

        public void Ok(object sender, RoutedEventArgs e) {
            var encounterSets = new List<EncounterSet>();
            foreach (var encounterSet in _selectableEncounterSets) {
                if (encounterSet.IsSelected) {
                    encounterSets.Add(encounterSet.EncounterSet);
                }
            }
            _appData.Game.EncounterSets = encounterSets;
            _appData.Game.OnEncounterSetsChanged();
            Close();
        }

        public void Cancel(object sender, RoutedEventArgs e) {
            Close();
        }
    }

    internal class SelectableEncounterCycles {
        public SelectableEncounterCycles() {
            Cycles = new List<SelectableEncounterCycle>();
        }

        public IList<SelectableEncounterCycle> Cycles { get; private set; }

        public SelectableEncounterCycle GetCyle(Pack pack) {
            while (Cycles.Count < pack.CyclePosition) {
                Cycles.Add(new SelectableEncounterCycle());
            }
            return Cycles[pack.CyclePosition - 1];
        }
    }

    internal class SelectableEncounterCycle {
        public SelectableEncounterCycle() {
            EncounterSets = new List<SelectableEncounterSet>();
        }

        public IList<SelectableEncounterSet> EncounterSets { get; private set; }
    }

    class SelectableEncounterSet {
        public SelectableEncounterSet(EncounterSet encounterSet) {
            EncounterSet = encounterSet;
        }

        public EncounterSet EncounterSet { get; private set; }
        public bool IsSelected { get; set; }
    }
}
