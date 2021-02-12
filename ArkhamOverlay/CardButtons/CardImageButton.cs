using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Data;
using ArkhamOverlay.Events;
using System.Windows.Media;

namespace ArkhamOverlay.CardButtons {
    public abstract class CardImageButton : Button {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();

        public CardImageButton(CardTemplate cardTemplate) {
            CardTemplate = cardTemplate;
            Text = cardTemplate.Name;
            IsToggled = cardTemplate.IsDisplayedOnOverlay;

            //todo: we need to find a way to unsubscribe to this gracefully- we cannnot put it in the destructor because as long as this
            //event is hooked, this object will not be garbage collected as the event bus will have a reference to it. I am hoping
            //with further refactoring we may see this logic move outside of this object altogether or some other answer will present itself
            _eventBus.SubscribeToCardTemplateVisibilityChangedEvent(e => {
                if (e.Name == cardTemplate.Name) {
                    IsToggled = e.IsVisible;
                }
            }); 
        }

        public CardTemplate CardTemplate { get; }

        public override ImageSource ButtonImage { get { return CardTemplate.ButtonImage; } }

        public override void LeftClick() {
            _eventBus.PublishToggleCardVisibilityRequest(CardTemplate);
        }
    }
}
