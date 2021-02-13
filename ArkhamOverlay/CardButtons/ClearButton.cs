using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Data;
using ArkhamOverlay.Events;

namespace ArkhamOverlay.CardButtons {
    public class ClearButton : Button {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        private readonly CardGroup _selectableCards;

        public ClearButton(CardGroup selectableCards) {
            Text = "Clear Cards";
            _selectableCards = selectableCards;
        }

        public override void LeftClick() {
            _eventBus.PublishClearAllCardsForCardGroupRequest(_selectableCards);
        }
    }
}
