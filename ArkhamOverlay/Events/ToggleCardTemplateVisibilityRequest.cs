using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Data;
using System;

namespace ArkhamOverlay.Events {
    public class ToggleCardTemplateVisibilityRequest : IEvent {
        public ToggleCardTemplateVisibilityRequest(CardTemplate cardTemplate) {
            CardTemplate = cardTemplate;
        }

        public CardTemplate CardTemplate { get; }
    }

    public static class ToggleCardVisibilityRequestExtensions {
        public static void PublishToggleCardTemplateVisibilityRequest(this IEventBus eventBus, CardTemplate cardTemplate) {
            eventBus.Publish(new ToggleCardTemplateVisibilityRequest(cardTemplate));
        }

        public static void SubscribeToToggleCardTemplateVisibilityRequest(this IEventBus eventBus, Action<ToggleCardTemplateVisibilityRequest> callback) {
            eventBus.Subscribe(callback);
        }

        public static void UnsubscribeFromToggleCardTemplateVisibilityRequest(this IEventBus eventBus, Action<ToggleCardTemplateVisibilityRequest> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
