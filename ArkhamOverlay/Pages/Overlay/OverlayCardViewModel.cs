using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Data;
using PageController;
using System.Windows;
using System.Windows.Media;

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
                CardImage = card.Image;

                NotifyPropertyChanged(nameof(CardImage));

                Show = true;
            }
        }

        private ICardInstance _cardInstance;
        public ICardInstance CardInstance { 
            get => _cardInstance; 
            set {
                _cardInstance = value;
                Card = _cardInstance.Card;
            } 
        }

        private bool _show;
        public virtual bool Show {
            get => _show; 
            set {
                _show = value;
                NotifyPropertyChanged(nameof(Show));
            }
        }

        public ImageSource CardImage { get; set; }

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
