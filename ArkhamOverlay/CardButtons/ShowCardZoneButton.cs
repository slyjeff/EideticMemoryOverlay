using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Data;
using ArkhamOverlay.Events;

namespace ArkhamOverlay.CardButtons {
    public class ShowCardZoneButton : Button {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();

        private readonly CardZone _cardZone;

        public ShowCardZoneButton(CardZone cardZone)  {
            _cardZone = cardZone;
        }

        public override void LeftClick() {
            _eventBus.PublishToggleCardZoneVisibilityRequest(_cardZone);
        }
    }
}