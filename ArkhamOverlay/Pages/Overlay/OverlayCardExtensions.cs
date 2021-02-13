using ArkhamOverlay.Data;
using System.Collections.Generic;
using System.Linq;

namespace ArkhamOverlay.Pages.Overlay {
    public static class OverlayCardExtensions {
        public static void AddOverlayCard(this IList<OverlayCardViewModel> cards, OverlayCardViewModel cardViewModel) {
            var insertIndex = cards.Count;

            if (cardViewModel.CardTemplate.Type == CardType.Agenda) {
                //add this directly to the left of the first act
                var firstAct = cards.FirstOrDefault(x => x.CardTemplate.Type == CardType.Act);
                if (firstAct != null) {
                    insertIndex = cards.IndexOf(firstAct);
                }
            }

            cards.Insert(insertIndex, cardViewModel);
        }

        public static OverlayCardViewModel FindCardTemplateToReplace(this IList<OverlayCardViewModel> overlayCards, CardTemplate card) {
            return overlayCards.FirstOrDefault(x => x.CardTemplate == card.FlipSideCard);
        }

        public static void RemoveOverlayCards(this IList<OverlayCardViewModel> overlayCards, params OverlayCardViewModel[] overlayCardsToRemove) {
            foreach (var overlayCard in overlayCardsToRemove) {
                overlayCards.Remove(overlayCard);
            }
        }
    }
}
