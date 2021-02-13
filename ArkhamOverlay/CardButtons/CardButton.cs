using ArkhamOverlay.Data;

namespace ArkhamOverlay.CardButtons {
    public class CardButton : CardImageButton, ICard {
        private readonly CardZone _cardSet;

        public CardButton(CardZone cardSet, CardTemplate card) : base(card) {
            _cardSet = cardSet;
        }

        public override void RightClick() {
            _cardSet.RemoveCard(this);
        }
    }
}
