using ArkhamOverlay.Data;
using PageController;
using System;
using System.Collections.Generic;
using System.IO;

namespace ArkhamOverlay.Pages.LocalImages {
    public class LocalImagesViewModel : ViewModel {
        public LocalImagesViewModel() {
            Packs = new List<LocalPack>();
        }

        public virtual Configuration Configuration { get; set; }

        public virtual IList<LocalPack> Packs { get; set; }

        public virtual LocalPack SelectedPack { get; set; }

        public virtual bool IsPackSelected { get; set; }
    }

    public class LocalPack : ViewModel {
        public LocalPack(string directory) {
            Directory = directory;
            Name = Path.GetFileName(directory);
        }

        private string _name;
        public virtual string Name {
            get => _name;
            set {
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public virtual string Directory { get; }

        public override string ToString() {
            return Name;
        }
    }
}
