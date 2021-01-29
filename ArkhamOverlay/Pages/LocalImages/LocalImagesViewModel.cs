using ArkhamOverlay.Data;
using Newtonsoft.Json;
using PageController;
using System.Collections.Generic;
using System.IO;

namespace ArkhamOverlay.Pages.LocalImages {
    public class LocalImagesViewModel : ViewModel {
        public LocalImagesViewModel() {
            Packs = new List<LocalPack>();
        }

        public virtual Configuration Configuration { get; set; }

        public IList<LocalPack> Packs { get; set; }
    }

    public class LocalPack : ViewModel {
        public LocalPack(string directory) {
            Directory = directory;
            Name = Path.GetFileName(directory);
        }

        public virtual string Name { get; set; }
        public virtual string Directory { get; }

        public override string ToString() {
            return Name;
        }
    }
}
