using System.Collections.Generic;

namespace EideticMemoryOverlay.PluginApi.LocalCards {
    public class LocalPackManifest {
        public LocalPackManifest() {
            Cards = new List<LocalCard>();
            PlugInName = "EmoPlugIn.ArkhamHorrorLcg.dll";
        }

        public string Name { get; set; }
        public string PlugInName { get; set; }

        public IList<LocalCard> Cards { get; set; }
    }

    public class LocalPackManifest<T> where T : LocalCard {
        public LocalPackManifest() {
            Cards = new List<T>();
            PlugInName = "EmoPlugIn.ArkhamHorrorLcg.dll";
        }

        public string Name { get; set; }
        public string PlugInName { get; set; }

        public IList<T> Cards { get; set; }
    }
}
