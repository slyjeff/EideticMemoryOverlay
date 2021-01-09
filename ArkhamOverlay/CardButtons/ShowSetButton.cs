using ArkhamOverlay.Data;

namespace ArkhamOverlay.CardButtons {
    public class ShowSetButton : CardButton {
        private readonly CardSet _cardSet;

        public ShowSetButton(CardSet cardSet) {
            Text = "";
            _cardSet = cardSet;
            _cardSet.IsDisplayedOnOverlayChanged += (isDisplayedOnOverlay) => IsToggled = isDisplayedOnOverlay;
        }

        public override void LeftClick() {
            _cardSet.ToggleVisibility();
        }
    }
}