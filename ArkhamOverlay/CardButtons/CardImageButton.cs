using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Data;
using ArkhamOverlay.Events;
using System.Windows.Media;

namespace ArkhamOverlay.CardButtons {
    public abstract class CardImageButton : CardButton {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();

        public CardImageButton(SelectableCards selectableCards, CardTemplate cardTemplate) : base(selectableCards) {
            CardTemplate = cardTemplate;
            Text = cardTemplate.Name;
            IsToggled = cardTemplate.IsDisplayedOnOverlay;

            cardTemplate.IsDisplayedOnOverlayChanged += (isDisplayedOnOverlay) => { 
                IsToggled = isDisplayedOnOverlay;
                selectableCards.OnButtonChanged(this);
            };
        }

        public CardTemplate CardTemplate { get; }

        public override ImageSource ButtonImage { get { return CardTemplate.ButtonImage; } }

        public override void LeftClick() {
            _eventBus.PublishToggleCardVisibilityRequest(CardTemplate);
        }
    }
}
