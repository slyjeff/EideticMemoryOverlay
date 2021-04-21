using Emo.Data;
using PageController;
using System;
using System.Windows;
using System.Windows.Media;

namespace Emo.Pages.Overlay {
    public enum OverlayCardType { Info, TopCardZone, BottomCardZone}

    public class OverlayCardViewModel : ViewModel {
        public static double CardWidthRatio = 0.716;
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
                    case OverlayCardType.Info:
                        return nameof(Configuration.CardHeight);
                    case OverlayCardType.TopCardZone:
                        return nameof(Configuration.TopCardZoneHeight);
                    case OverlayCardType.BottomCardZone:
                        return nameof(Configuration.BottomCardZoneHeight);
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

        private CardInfo _cardInfo;
        public CardInfo CardInfo {
            get => _cardInfo;
            set {
                _cardInfo = value;

                CardImage = _cardInfo.Image;
                NotifyPropertyChanged(nameof(CardImage));
            }
        }

        private ICard _card;
        public ICard Card { 
            get => _card; 
            set {
                _card = value;
                CardInfo = _card.CardInfo;
            } 
        }

        public ImageSource CardImage { get; set; }

        public double Height {
            get {
                return CardInfo.IsHorizontal ? Math.Min(MaxHeight, ConfigurationHeight) * CardWidthRatio : Math.Min(MaxHeight, ConfigurationHeight);
            }
        }

        public double Width {
            get {
                return CardInfo.IsHorizontal ? Height / CardWidthRatio : Height * CardWidthRatio;
            }
        }

        public double Radius { get { return (CardInfo.IsHorizontal ? Width : Height) / _cardRadiusDivisor; } }

        public Rect ClipRect {get { return new Rect { Height = Height, Width = Width }; } }

        public double Margin { get => 5; }
    }
}
