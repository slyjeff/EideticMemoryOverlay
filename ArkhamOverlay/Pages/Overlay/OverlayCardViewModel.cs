using ArkhamOverlay.Data;
using PageController;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ArkhamOverlay.Pages.Overlay {
    public class OverlayCardViewModel : ViewModel {
        private readonly Configuration _configuartion;

        public OverlayCardViewModel(Configuration configuartion) {
            _configuartion = configuartion;
            _configuartion.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(_configuartion.CardHeight)) {
                    NotifyPropertyChanged(nameof(Height));
                    NotifyPropertyChanged(nameof(Width));
                    NotifyPropertyChanged(nameof(Radius));
                    NotifyPropertyChanged(nameof(ClipRect));
                }
            };
        }

        private Card card;

        public Card Card {
            get => card;
            set {
                card = value;
                CardImage = new BitmapImage(new Uri("https://arkhamdb.com/" + card.ImageSource, UriKind.Absolute));

                NotifyPropertyChanged(nameof(CardImage));

                Visibility = Visibility.Visible;
                NotifyPropertyChanged(nameof(Visibility));
            }
        }

        public Visibility Visibility { get; set; }

        public BitmapImage CardImage { get; set; }

        public double Height {
            get {
                return card.IsHorizontal ? _configuartion.CardWidth : _configuartion.CardHeight;
            }
        }

        public double Width {
            get {
                return card.IsHorizontal ? _configuartion.CardHeight : _configuartion.CardWidth;
            }
        }

        public double Radius { get { return _configuartion.CardRadius; } }

        public Rect ClipRect {
            get {
                return card.IsHorizontal
                    ? new Rect { Height = _configuartion.CardClipRect.Width, Width = _configuartion.CardClipRect.Height }
                    : _configuartion.CardClipRect;
            }
        }
    }
}
