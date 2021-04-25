namespace EideticMemoryOverlay.PluginApi {
    public class EncounterSet {
        public EncounterSet() {
        }

        public EncounterSet(EncounterSet encounterSet) {
            Name = encounterSet.Name;
            Code = encounterSet.Code;
        }

        public string Name { get; set; }
        public string Code { get; set; }
    }
}
