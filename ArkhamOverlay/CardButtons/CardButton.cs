using ArkhamOverlay.Data;

namespace ArkhamOverlay.CardButtons {
    public class CardButton : CardImageButton, ICard {
        public CardButton(CardInfoButton cardInfoButton) : base(cardInfoButton.CardInfo, cardInfoButton.IsToggled) {
        }
    }
}
