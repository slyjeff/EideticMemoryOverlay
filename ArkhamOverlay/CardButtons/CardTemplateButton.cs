using ArkhamOverlay.Data;
using System.Windows.Media;

namespace ArkhamOverlay.CardButtons {
    public class CardTemplateButton : CardImageButton {
        private readonly CardGroup _selectableCards;

        public CardTemplateButton(CardGroup selectableCards, CardTemplate cardTemplate) : base(cardTemplate, false) {
            _selectableCards = selectableCards;
        }

        public override void RightClick() {
            var cardZone = _selectableCards.CardZone;
            if (cardZone == default(CardZone)) {
                return;
            }

            cardZone.AddCard(CardTemplate, IsToggled);
        }

        public override ImageSource ButtonImage { get { return CardTemplate.ButtonImage; } }
    }
}
