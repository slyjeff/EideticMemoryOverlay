using ArkhamOverlay.Data;

namespace ArkhamOverlay.CardButtons {
    public class CardButton : CardImageButton, ICard {
        public CardButton(CardImageButton button) : base(button.CardInfo, button.IsToggled) {
        }
    }
}
