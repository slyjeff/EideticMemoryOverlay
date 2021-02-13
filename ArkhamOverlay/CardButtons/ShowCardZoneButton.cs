using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Data;
using ArkhamOverlay.Events;

namespace ArkhamOverlay.CardButtons {
    public class ShowCardZoneButton : Button {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();

        private readonly CardGroup _cardGroup;

        public ShowCardZoneButton(CardGroup cardGroup)  {
            _cardGroup = cardGroup;

            cardGroup.CardZone.IsDisplayedOnOverlayChanged += (isDisplayedOnOverlay) => {
                IsToggled = isDisplayedOnOverlay;
                _eventBus.PublishButtonToggled(_cardGroup.Id, 0, _cardGroup.CardButtons.IndexOf(this), IsToggled);
            };
        }

        public override void LeftClick() {
            _eventBus.PublishToggleCardZoneVisibilityRequest(_cardGroup.CardZone);
        }
    }
}