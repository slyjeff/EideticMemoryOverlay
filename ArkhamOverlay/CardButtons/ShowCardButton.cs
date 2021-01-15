using ArkhamOverlay.Data;
using System.Windows.Media;

namespace ArkhamOverlay.CardButtons {
    public class ShowCardButton : CardImageButton {
        private readonly SelectableCards _selectableCards;

        public ShowCardButton(SelectableCards selectableCards, Card card) : base(selectableCards, card) {
            _selectableCards = selectableCards;
        }

        public override void RightClick() {
            //we only put act/agend/player cards in sets
            if ((Card.Type != CardType.Act) && (Card.Type != CardType.Agenda) && !Card.IsPlayerCard) {
                return;
            }

            _selectableCards.CardSet.AddCard(Card);
        }

        public override ImageSource ButtonImage { get { return Card.ButtonImage; } }
    }
}
