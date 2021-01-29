using ArkhamOverlay.Data;
using PageController;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace ArkhamOverlay.Pages.LocalImages {
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
            Cards = new ObservableCollection<LocalCard>();
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

        private LocalCard _selectedCard;
        public virtual LocalCard SelectedCard {
            get => _selectedCard;
            set {
                _selectedCard = value;
                NotifyPropertyChanged(nameof(SelectedCard));
                NotifyPropertyChanged(nameof(IsCardSelected));
            }
        }

        public virtual bool IsCardSelected { get { return SelectedCard != null; } }

        public virtual ObservableCollection<LocalCard> Cards { get; set; }
    }

    public class LocalCard : ViewModel {
        public LocalCard(string path) {
            FilePath = path;
            Name = Path.GetFileNameWithoutExtension(path);
        }

        public virtual string FilePath { get; }

        private string _name;
        public virtual string Name{
            get => _name;
            set {
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        private string _cardType;
        public virtual string CardType {
            get => _cardType;
            set {
                _cardType = value;
                NotifyPropertyChanged(nameof(CardType));
            }
        }

        public virtual bool HasBack { get; set; }

        public virtual ImageSource Image { get; set; }
        public virtual ImageSource FrontThumbnail { get; set; }
        public virtual ImageSource BackThumbnail { get; set; }

        public Rect ClipRect { get; set; }
    }
}
