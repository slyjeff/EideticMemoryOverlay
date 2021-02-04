using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class ModeToggled : IEvent {
    }

    public static class ModeToggledExtensions {
        public static void ToggleMode(this IEventBus eventBus) {
            eventBus.Publish(new ModeToggled());
        }

        public static void Subscribe(this IEventBus eventBus, Action<ModeToggled> action) {
            eventBus.Subscribe(action);
        }

        public static void Unsubscribe(this IEventBus eventBus, Action<ModeToggled> action) {
            eventBus.Unsubscribe(action);
        }
    }
}
