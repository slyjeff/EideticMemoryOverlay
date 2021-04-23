using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.LocalCards;
using Emo.Data;
using PageController;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace Emo.Pages.LocalImages {
    public class LocalImagesViewModel : ViewModel {
        public LocalImagesViewModel() {
            Packs = new List<LocalPack>();
            CardTypes = new List<string> { "Asset", "Event", "Skill", "Scenario", "Agenda", "Act", "Enemy", "Treachery", "Location", "Investigator" };
        }

        public virtual Configuration Configuration { get; set; }

        public virtual IList<LocalPack> Packs { get; set; }

        private LocalPack _selectedPack;
        public virtual LocalPack SelectedPack {
            get => _selectedPack;
            set {
                _selectedPack = value;
                NotifyPropertyChanged(nameof(SelectedPack));
                NotifyPropertyChanged(nameof(IsPackSelected));
            }
        }

        public virtual bool IsPackSelected { get { return SelectedPack != null; } }

        public virtual IList<string> CardTypes { get; }
    }

    public class LocalPack : ViewModel {
        public LocalPack(string directory) {
            Directory = directory;
            Name = Path.GetFileName(directory);
            Cards = new ObservableCollection<EditableLocalCard>();
        }

        public virtual string Directory { get; }

        private string _name;
        public virtual string Name {
            get => _name;
            set {
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        private EditableLocalCard _selectedCard;
        public virtual EditableLocalCard SelectedCard {
            get => _selectedCard;
            set {
                _selectedCard = value;
                NotifyPropertyChanged(nameof(SelectedCard));
                NotifyPropertyChanged(nameof(IsCardSelected));
            }
        }

        public virtual bool IsCardSelected { get { return SelectedCard != null; } }

        public virtual ObservableCollection<EditableLocalCard> Cards { get; set; }
    }
}
