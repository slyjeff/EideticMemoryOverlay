using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace ArkhamOverlay {
    public class OverlayData {
        public OverlayData() {
            Cards = new ObservableCollection<OverlayCard>();
        }

        public Configuration Configuration { get; set; }

        public ObservableCollection<OverlayCard> Cards { get; set; }
    }

    public class OverlayCard : INotifyPropertyChanged {
        private Card card;

        public Card Card {
            get => card;
            set {
                card = value;
                CardImage = new BitmapImage(new Uri("https://arkhamdb.com/" + card.ImageSource, UriKind.Absolute));
                OnPropertyChanged("CardImage");

                Visibility = Visibility.Visible;
                OnPropertyChanged("Visibility");
            }
        }

        public Visibility Visibility { get; set; }

        public BitmapImage CardImage { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler == null) {
                return;
            }
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
