using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ArkhamOverlay {
    public partial class Overlay : Window {
        private Card _card;

        public Overlay() {
            InitializeComponent();
        }

        internal void ToggleCard(Card card) {
            if (_card == card) {
                Card.Source = null;
                _card = null;
                return;
            }

            _card = card;

            Card.Source = new BitmapImage(new Uri("https://arkhamdb.com/" + card.ImageSource, UriKind.Absolute));
        }
    }
}
