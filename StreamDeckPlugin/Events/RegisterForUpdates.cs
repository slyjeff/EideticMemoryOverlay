using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class RegisterForUpdates : IEvent {
    }

    public static class RegisterForUpdatesExtensions {
        public static void RegisterForUpdates(this IEventBus eventBus) {
            eventBus.Publish(new RegisterForUpdates());
        }

        public static void OnRegisterForUpdates(this IEventBus eventBus, Action action) {
            eventBus.Subscribe<RegisterForUpdates>(x => action?.Invoke());
        }
    }
}
