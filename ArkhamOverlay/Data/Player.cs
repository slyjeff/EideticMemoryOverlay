using ArkhamOverlay.Utils;
using PageController;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ArkhamOverlay.Data {
    public class Player : ViewModel {
        private static readonly Dictionary<string, BitmapImage> CardImageCache = new Dictionary<string, BitmapImage>();
        Configuration _configuration;

        public Player(Configuration configuration, int id) {
            _configuration = configuration;
            ID = id;
            SelectableCards = new SelectableCards(SelectableType.Player);
            Health = new Stat("health.png");
            Sanity = new Stat("sanity.png");
            Resources = new Stat("resource.png");
            Clues = new Stat("clue.png");
            Faction = Faction.Other;

            Resources.Value = 5;

            configuration.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(Configuration.TrackPlayerStats)) {
                    NotifyPropertyChanged(nameof(StatTrackingVisibility));
                }
            };
        }

        internal void LoadInvestigatorImage(string url) {
            if (Application.Current.Dispatcher.CheckAccess()) {
                LoadImage(url);
            } else {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    LoadImage(url);
                }));
            }
        }
    
        private void LoadImage(string url) {
            if (CardImageCache.ContainsKey(Name)) {
                InvestigatorImage = CardImageCache[Name];
                CropImage();
                return;
            }

            var bitmapImage = new BitmapImage(new Uri(url, UriKind.Absolute));
            bitmapImage.DownloadCompleted += (s, e) => {
                CardImageCache[Name] = bitmapImage;
                CropImage();
            };
            InvestigatorImage = bitmapImage;
        }

        internal void CropImage() {
            InvestigatorButtonImage = InvestigatorImage.CropImage(CardType.Investigator);
            InvestigatorButtonImageAsBytes = InvestigatorButtonImage.AsBytes();
            NotifyPropertyChanged(nameof(InvestigatorButtonImageAsBytes));
        }

        public int ID { get; }

        public string DeckId { get; set; }

        public SelectableCards SelectableCards { get; }

        public string Name { get { return SelectableCards.Name; } }

        public ImageSource InvestigatorImage { get; set; }

        public ImageSource InvestigatorButtonImage { get; private set; }
        
        public byte[] InvestigatorButtonImageAsBytes { get; private set; }

        public string InvestigatorCode { get; set; }

        public IDictionary<string, int> Slots { get; set; }

        public Visibility LoadedVisiblity { get { return string.IsNullOrEmpty(SelectableCards.Name) ? Visibility.Hidden : Visibility.Visible; } }

        public void LoadImage() {

        }

        public void OnPlayerChanged() {
            NotifyPropertyChanged(nameof(LoadedVisiblity));
            NotifyPropertyChanged(nameof(InvestigatorImage));
            NotifyPropertyChanged(nameof(Name));
            NotifyPropertyChanged(nameof(PlayerNameBrush));
            NotifyPropertyChanged(nameof(StatTrackingVisibility));
        }

        public Stat Health { get; }
        public Stat Sanity { get; }
        public Stat Resources { get; }
        public Stat Clues { get; }
        
        public Visibility StatTrackingVisibility { get { return string.IsNullOrEmpty(SelectableCards.Name) || !_configuration.TrackPlayerStats ? Visibility.Collapsed : Visibility.Visible; } }
        public Brush PlayerNameBrush {
            get {
                switch (Faction) {
                    case Faction.Guardian: return new SolidColorBrush(Colors.DodgerBlue);
                    case Faction.Seeker: return new SolidColorBrush(Colors.DarkGoldenrod);
                    case Faction.Mystic: return new SolidColorBrush(Colors.MediumPurple);
                    case Faction.Rogue: return new SolidColorBrush(Colors.MediumSpringGreen);
                    case Faction.Survivor: return new SolidColorBrush(Colors.Red);
                    default: return new SolidColorBrush(Colors.Black);
                }
            }
        }

        public Faction Faction { get; set; }
    }

    public class Stat : ViewModel {
        public Stat(string imageFile) {
            var fileName = AppDomain.CurrentDomain.BaseDirectory + "Images\\" + imageFile;
            Image = new BitmapImage(new Uri(fileName));
            Increase = new UpdateStateCommand(this, true);
            Decrease = new UpdateStateCommand(this, false);
        }

        public ImageSource Image { get; }

        private int _value;
        public int Value { 
            get => _value;
            set {
                _value = value;
                NotifyPropertyChanged(nameof(Value));
            }
        }

        public ICommand Increase { get; }
        public ICommand Decrease { get; }
    }

    public class UpdateStateCommand : ICommand {
        private Stat _stat;
        private bool _increase;
        public UpdateStateCommand(Stat stat, bool increase) {
            _stat = stat;
            _increase = increase;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            if (_increase) {
                _stat.Value++;
                return;
            }

            if (_stat.Value > 0) {
                _stat.Value--;
            } 
        }
    }
}