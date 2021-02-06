using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class RegisterForUpdatesRequest : IEvent {
    }

    public static class RegisterForUpdatesRequestExtensions {
        public static void PublishRegisterForUpdatesRequest(this IEventBus eventBus) {
            eventBus.Publish(new RegisterForUpdatesRequest());
        }

        public static void SubscribeToRegisterForUpdatesRequest(this IEventBus eventBus, Action<RegisterForUpdatesRequest> callback) {
            eventBus.Subscribe<RegisterForUpdatesRequest>(callback);
        }
    }
}
