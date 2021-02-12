using ArkhamOverlay.Data;
using System.Windows.Media;

namespace ArkhamOverlay.CardButtons {
    public class CardTemplateButton : CardImageButton {
        private readonly SelectableCards _selectableCards;

        public CardTemplateButton(SelectableCards selectableCards, CardTemplate cardTemplate) : base(cardTemplate) {
            _selectableCards = selectableCards;
        }

        public override void RightClick() {
            //we only put act/agend/player cards in sets
            if ((CardTemplate.Type != CardType.Act) && (CardTemplate.Type != CardType.Agenda) && !CardTemplate.IsPlayerCard) {
                return;
            }

            _selectableCards.CardZone.AddCard(CardTemplate);
        }

        public override ImageSource ButtonImage { get { return CardTemplate.ButtonImage; } }
    }
}
