using ArkhamOverlay.Data;

namespace ArkhamOverlay.CardButtons {
    public class ShowSetButton : CardButton {
        private readonly SelectableCards _selectableCards;

        public ShowSetButton(SelectableCards selectableCards) : base (selectableCards) {
            _selectableCards = selectableCards;
            selectableCards.CardSet.IsDisplayedOnOverlayChanged += (isDisplayedOnOverlay) => {
                IsToggled = isDisplayedOnOverlay;
                selectableCards.OnButtonChanged(this);
            };
        }

        public override void LeftClick() {
            _selectableCards.CardSet.ToggleVisibility();
        }
    }
}