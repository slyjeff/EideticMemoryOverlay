using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Data;
using System;

namespace ArkhamOverlay.Events {
    public class ToggleCardVisibilityRequest : IEvent {
        public ToggleCardVisibilityRequest(CardTemplate cardTemplate) {
            CardTemplate = cardTemplate;
        }

        public CardTemplate CardTemplate { get; }
    }

    public static class ToggleCardVisibilityRequestExtensions {
        public static void PublishToggleCardVisibilityRequest(this IEventBus eventBus, CardTemplate cardTemplate) {
            eventBus.Publish(new ToggleCardVisibilityRequest(cardTemplate));
        }

        public static void SubscribeToToggleCardVisibilityRequest(this IEventBus eventBus, Action<ToggleCardVisibilityRequest> callback) {
            eventBus.Subscribe(callback);
        }

        public static void UnsubscribeFromToggleCardVisibilityRequest(this IEventBus eventBus, Action<ToggleCardVisibilityRequest> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
