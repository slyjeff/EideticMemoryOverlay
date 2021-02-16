using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Events {
    public class StatTrackingVisibilityChangedEvent : IEvent {
        public StatTrackingVisibilityChangedEvent(bool isVisible) {
            IsVisible = isVisible;
        }

        public bool IsVisible { get; }
    }

    public static class StatTrackingVisibilityChangedEventExtensions {
        public static void PublishStatTrackingVisibilityChangedEvent(this IEventBus eventBus, bool isVisible) {
            eventBus.Publish(new StatTrackingVisibilityChangedEvent(isVisible));
        }

        public static void SubscribeToStatTrackingVisibilityChangedEvent(this IEventBus eventBus, Action<StatTrackingVisibilityChangedEvent> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
