using ArkhamOverlay.Data;

namespace ArkhamOverlay.CardButtons {
    public class CardInSetButton : CardImageButton, ICardInstance {
        private readonly CardSet _cardSet;

        public CardInSetButton(CardSet cardSet, SelectableCards selectableCards, Card card) : base(selectableCards, card) {
            _cardSet = cardSet;
        }

        public override void RightClick() {
            _cardSet.RemoveCard(this);
        }
    }
}
