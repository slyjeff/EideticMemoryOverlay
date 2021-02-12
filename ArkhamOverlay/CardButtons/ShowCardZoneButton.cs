using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Data;
using ArkhamOverlay.Events;

namespace ArkhamOverlay.CardButtons {
    public class ShowCardZoneButton : Button {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();

        private readonly SelectableCards _selectableCards;

        public ShowCardZoneButton(SelectableCards selectableCards)  {
            _selectableCards = selectableCards;

            selectableCards.CardSet.IsDisplayedOnOverlayChanged += (isDisplayedOnOverlay) => {
                IsToggled = isDisplayedOnOverlay;
                _eventBus.PublishButtonToggledEvent(selectableCards.CardGroup, 0, selectableCards.CardButtons.IndexOf(this), IsToggled);
            };
        }

        public override void LeftClick() {
            _selectableCards.CardSet.ToggleVisibility();
        }
    }
}