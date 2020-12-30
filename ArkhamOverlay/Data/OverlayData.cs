using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ArkhamOverlay.Data {
    public class OverlayData {
        public OverlayData() {
            EncounterCards = new ObservableCollection<OverlayCard>();
            PlayerCards = new ObservableCollection<OverlayCard>();
        }

        public Configuration Configuration { get; set; }

        public ObservableCollection<OverlayCard> EncounterCards { get; set; }
        public ObservableCollection<OverlayCard> PlayerCards { get; set; }

        public void ClearPlayerCards(string id) {
            if(string.IsNullOrEmpty(id)) {
                return;
            }

            foreach (var card in PlayerCards.ToList()) {
                if(card.Card.OwnerId == id) {
                    PlayerCards.Remove(card);
                }
            }
        }

        public void ClearScenarioCards() {
            foreach (var card in EncounterCards.ToList()) {
                if (card.Card.Type == CardType.Scenario || card.Card.Type == CardType.Agenda || card.Card.Type == CardType.Act) {
                    EncounterCards.Remove(card);
                }
            }
        }

        public void ClearEncounterCards() {
            foreach (var card in EncounterCards.ToList()) {
                if (card.Card.Type == CardType.Enemy || card.Card.Type == CardType.Treachery) {
                    EncounterCards.Remove(card);
                }
            }
        }

        public void ClearLocationCards() {
            foreach (var card in EncounterCards.ToList()) {
                if (card.Card.Type == CardType.Location) {
                    EncounterCards.Remove(card);
                }
            }
        }
    }

    public class OverlayCard : INotifyPropertyChanged {
        private readonly Configuration _configuartion;

        public OverlayCard(Configuration configuartion) {
            _configuartion = configuartion;
            _configuartion.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(_configuartion.CardHeight)) {
                    OnPropertyChanged(nameof(Height));
                    OnPropertyChanged(nameof(Width));
                    OnPropertyChanged(nameof(Radius));
                    OnPropertyChanged(nameof(ClipRect));
                }
            };
        }

        private Card card;

        public Card Card {
            get => card;
            set {
                card = value;
                CardImage = new BitmapImage(new Uri("https://arkhamdb.com/" + card.ImageSource, UriKind.Absolute));
                OnPropertyChanged(nameof(CardImage));

                Visibility = Visibility.Visible;
                OnPropertyChanged(nameof(Visibility));
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
