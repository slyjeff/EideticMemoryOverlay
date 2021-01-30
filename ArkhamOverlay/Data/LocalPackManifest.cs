using System.Collections.Generic;

namespace ArkhamOverlay.Data {
    public class LocalPackManifest {
        public LocalPackManifest() {
            Cards = new List<LocalManifestCard>();   
        }

        public string Name { get; set; }

        public IList<LocalManifestCard> Cards { get; set; }
    }

    public class LocalManifestCard {
        public string FilePath { get; set; }
        public string Name { get; set; }
        public bool HasBack { get; set; }
        public string CardType { get; set; }
    }
}
