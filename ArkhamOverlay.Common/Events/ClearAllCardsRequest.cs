using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Common.Events {
    public class ClearAllCardsRequest : ICrossAppEvent {
    }

    public static class ClearAllCardsRequestExtensions {
        public static void PublishClearAllCardsRequest(this IEventBus eventBus) {
            eventBus.Publish(new ClearAllCardsRequest());
        }

        public static void SubscribeToClearAllCardsRequest(this IEventBus eventBus, Action<ClearAllCardsRequest> callback) {
            eventBus.Subscribe(callback);
        }

        public static void UnsubscribeFromClearAllCardsRequest(this IEventBus eventBus, Action<ClearAllCardsRequest> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
