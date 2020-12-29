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
                cards.Add(new OverlayCard(overlayData.Configuration) { Card = card } );
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
