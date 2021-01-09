using ArkhamOverlay.Data;
using System.Windows.Media;

namespace ArkhamOverlay.CardButtons {
    public abstract class CardImageButton : CardButton {
        private readonly SelectableCards _selectableCards;
        public CardImageButton(SelectableCards selectableCards, Card card) {
            _selectableCards = selectableCards;

            Card = card;
            Text = card.Name;

            card.IsDisplayedOnOverlayChanged += (isDisplayedOnOverlay) => { IsToggled = isDisplayedOnOverlay; };
        }

        public Card Card { get; }

        public override ImageSource ButtonImage { get { return Card.ButtonImage; } }

        public override void LeftClick() {
            _selectableCards.ToggleCardVisibility(Card);
        }
    }
}
