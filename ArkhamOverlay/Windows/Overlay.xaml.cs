using ArkhamOverlay.Data;
using System.Linq;
using System.Windows;

namespace ArkhamOverlay {
    public partial class Overlay : Window {
        public Overlay() {
            InitializeComponent();
        }

        internal void ToggleCard(Card card) {
            var overlayData = DataContext as OverlayData;
            var cards = card.IsPlayerCard ? overlayData.PlayerCards : overlayData.EncounterCards;

            var existingCard = cards.FirstOrDefault(x => x.Card == card);
            if (existingCard == null) {
                var newCard = new OverlayCard(overlayData.Configuration) { Card = card };
                var cardToReplace = cards.FirstOrDefault(x => x.Card.Code == card.Code);
                if (cardToReplace == null) {
                    cards.Add(newCard);
                } else {
                    cards[cards.IndexOf(cardToReplace)] = newCard;

                }
            } else {
                cards.Remove(existingCard);
            }
        }

        internal void ClearCards() {
            var overlayData = DataContext as OverlayData;

            overlayData.EncounterCards.Clear();
            overlayData.PlayerCards.Clear();
        }
    }
}
