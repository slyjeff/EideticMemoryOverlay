using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class ClearAllCardsRequest : IEvent {
    }

    public static class ClearAllCardsRequestExtensions {
        public static void PublicClearAllCardsRequest(this IEventBus eventBus) {
            eventBus.Publish(new ClearAllCardsRequest());
        }

        public static void SubscribeToClearAllCardsRequest(this IEventBus eventBus, Action<ClearAllCardsRequest> callback) {
            eventBus.Subscribe<ClearAllCardsRequest>(callback);
        }
    }
}
