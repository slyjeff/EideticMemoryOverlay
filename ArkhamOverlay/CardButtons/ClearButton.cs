using ArkhamOverlay.Data;

namespace ArkhamOverlay.CardButtons {
    public class ClearButton : CardButton {
        private readonly SelectableCards _selectableCards;

        public ClearButton(SelectableCards selectableCards) {
            Text = "Clear Cards";
            _selectableCards = selectableCards;
        }

        public override void LeftClick() {
            _selectableCards.HideAllCards();
        }
    }
}
