using System.Collections.Generic;

namespace ArkhamOverlay.Services {
    public class ArkhamDbDeck {
        public string Id { get; set; }

        public string Investigator_Code { get; set; }

        public string Investigator_Name { get; set; }

        public Dictionary<string, int> Slots { get; set; }
    }
}
