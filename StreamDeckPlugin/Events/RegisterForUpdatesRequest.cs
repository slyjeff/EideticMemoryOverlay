using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class RegisterForUpdatesRequest : IEvent {
    }

    public static class RRegisterForUpdatesRequestExtensions {
        public static void RegisterForUpdates(this IEventBus eventBus) {
            eventBus.Publish(new RegisterForUpdatesRequest());
        }

        public static void SubscribeToRegisterForUpdatesRequest(this IEventBus eventBus, Action<RegisterForUpdatesRequest> callback) {
            eventBus.Subscribe<RegisterForUpdatesRequest>(callback);
        }
    }
}
