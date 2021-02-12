using ArkhamOverlay.Data;

namespace ArkhamOverlay.CardButtons {
    public class CardButton : CardImageButton, ICardInstance {
        private readonly CardZone _cardSet;

        public CardButton(CardZone cardSet, SelectableCards selectableCards, CardTemplate card) : base(card) {
            _cardSet = cardSet;
        }

        public override void RightClick() {
            _cardSet.RemoveCard(this);
        }
    }
}
