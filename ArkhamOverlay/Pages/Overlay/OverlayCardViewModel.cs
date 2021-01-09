using ArkhamOverlay.Data;
using PageController;
using System;
using System.Windows;
using System.Windows.Media;

namespace ArkhamOverlay.Pages.Overlay {
    public enum OverlayCardType { Display, ActAgenda, Hand,}

    public class OverlayCardViewModel : ViewModel {
        private const double _cardWidthRatio = 0.716;
        private const double _cardRadiusDivisor = 30;

        private readonly Configuration _configuartion;

        public OverlayCardViewModel(Configuration configuartion, OverlayCardType overlayCardType) {
            _configuartion = configuartion;
            OverlayCardType = overlayCardType;

            _configuartion.PropertyChanged += (s, e) => {
                if (e.PropertyName == HeightProperty) {
                    NotifyPropertyChanged(nameof(Height));
                    NotifyPropertyChanged(nameof(Width));
                    NotifyPropertyChanged(nameof(Radius));
                    NotifyPropertyChanged(nameof(ClipRect));
                }
            };
        }

        public OverlayCardType OverlayCardType { get; }
        public string HeightProperty { 
            get {
                switch (OverlayCardType) {
                    case OverlayCardType.Display:
                        return nameof(Configuration.CardHeight);
                    case OverlayCardType.ActAgenda:
                        return nameof(Configuration.ActAgendaCardHeight);
                    case OverlayCardType.Hand:
                        return nameof(Configuration.HandCardHeight);
                    default:
                        return nameof(Configuration.CardHeight);
                }
            }
        }

        private double ConfigurationHeight {
            get {
                return (int)_configuartion.GetType().GetProperty(HeightProperty).GetValue(_configuartion, null);
            }
        }

        private double _maxHeight = double.MaxValue;
        public double MaxHeight { 
            get => _maxHeight;
            set {
                _maxHeight = value;
                NotifyPropertyChanged(nameof(Height));
                NotifyPropertyChanged(nameof(Width));
                NotifyPropertyChanged(nameof(Radius));
                NotifyPropertyChanged(nameof(ClipRect));
            }
        }

        private Card _card;
        public Card Card {
            get => _card;
            set {
                _card = value;
                CardImage = _card.Image;

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
                return Math.Min(MaxHeight, Card.IsHorizontal ? ConfigurationHeight * _cardWidthRatio : ConfigurationHeight);
            }
        }

        public double Width {
            get {
                return Card.IsHorizontal ? Height / _cardWidthRatio : Height * _cardWidthRatio;
            }
        }

        public double Radius { get { return Height / _cardRadiusDivisor; } }

        public Rect ClipRect {get { return new Rect { Height = Height, Width = Width }; } }

        public double Margin { get => 5; }
    }
}
