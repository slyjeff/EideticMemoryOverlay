using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Data;
using System;

namespace ArkhamOverlay.Events {
    public class ToggleCardZoneVisibilityRequest : IEvent {
        public ToggleCardZoneVisibilityRequest(CardZone cardZone) {
            CardZone = cardZone;
        }

        public CardZone CardZone { get; }
    }

    public static class ToggleCardZoneVisibilityRequestExtensions {
        public static void PublishToggleCardZoneVisibilityRequest(this IEventBus eventBus, CardZone cardZone) {
            eventBus.Publish(new ToggleCardZoneVisibilityRequest(cardZone));
        }

        public static void SubscribeToToggleCardZoneVisibilityRequest(this IEventBus eventBus, Action<ToggleCardZoneVisibilityRequest> callback) {
            eventBus.Subscribe(callback);
        }

        public static void UnsubscribeFromToggleCardZoneVisibilityRequest(this IEventBus eventBus, Action<ToggleCardZoneVisibilityRequest> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
