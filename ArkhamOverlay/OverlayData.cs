using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace ArkhamOverlay {
    public class OverlayData {
        public Configuration Configuration { get; set; }

        public IList<OverlayCard> Cards { get; set; }
    }

    public class OverlayCard : INotifyPropertyChanged {
        private Card card;

        public Card Card {
            get => card;
            set {
                card = value;
                CardImage = new BitmapImage(new Uri("https://arkhamdb.com/" + card.ImageSource, UriKind.Absolute));
                OnPropertyChanged("CardImage");
            }
        }

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
