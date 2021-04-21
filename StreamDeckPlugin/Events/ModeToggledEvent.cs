using Emo.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class ModeToggledEvent : IEvent {
    }

    public static class ModeToggledEventExtensions {
        public static void PublishModeToggledEvent(this IEventBus eventBus) {
            eventBus.Publish(new ModeToggledEvent());
        }

        public static void SubscribeToModeToggledEvent(this IEventBus eventBus, Action<ModeToggledEvent> callback) {
            eventBus.Subscribe(callback);
        }

        public static void UnsubscribeFromModeToggledEvent(this IEventBus eventBus, Action<ModeToggledEvent> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
