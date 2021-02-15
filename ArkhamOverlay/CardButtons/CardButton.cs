using ArkhamOverlay.Data;

namespace ArkhamOverlay.CardButtons {
    public class CardButton : CardImageButton, ICard {
        public CardButton(CardTemplateButton cardTemplateButton) : base(cardTemplateButton.CardTemplate, cardTemplateButton.IsToggled) {
        }
    }
}
