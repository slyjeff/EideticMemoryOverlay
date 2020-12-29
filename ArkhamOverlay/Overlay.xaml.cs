using System.Linq;
using System.Windows;

namespace ArkhamOverlay {
    public partial class Overlay : Window {
        public Overlay() {
            InitializeComponent();
        }

        internal void ToggleCard(Card card) {
            var overlayData = DataContext as OverlayData;

            var existingCard = overlayData.Cards.FirstOrDefault(x => x.Card == card);
            if (existingCard == null) {
                overlayData.Cards.Add(new OverlayCard { Card = card });
            } else {
                overlayData.Cards.Remove(existingCard);
            }
        }

        internal void ClearCards() {
            var overlayData = DataContext as OverlayData;

            overlayData.Cards.Clear();
        }
    }
}
