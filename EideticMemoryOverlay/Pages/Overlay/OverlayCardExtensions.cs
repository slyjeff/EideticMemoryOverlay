using EideticMemoryOverlay.PluginApi;
using System.Collections.Generic;
using System.Linq;

namespace Emo.Pages.Overlay {
    public static class OverlayCardExtensions {
        public static void AddOverlayCard(this IList<OverlayCardViewModel> cards, OverlayCardViewModel cardViewModel) {
            var insertIndex = cards.Count;

            //todo: a place where we can add hooks to determine the order things are inserted- originally implemented to sort Arkham Agendas before acts.

            cards.Insert(insertIndex, cardViewModel);
        }

        public static OverlayCardViewModel FindCardInfoToReplace(this IList<OverlayCardViewModel> overlayCards, CardInfo card) {
            return overlayCards.FirstOrDefault(x => x.CardInfo == card.FlipSideCard);
        }

        public static void RemoveOverlayCards(this IList<OverlayCardViewModel> overlayCards, params OverlayCardViewModel[] overlayCardsToRemove) {
            foreach (var overlayCard in overlayCardsToRemove) {
                overlayCards.Remove(overlayCard);
            }
        }
    }
}
