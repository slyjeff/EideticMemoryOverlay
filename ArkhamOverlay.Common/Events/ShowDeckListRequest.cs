using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Common.Events {
    public class ShowDeckListRequest : ICrossAppEvent {
        public ShowDeckListRequest(CardGroupId cardGroupId) {
            CardGroupId = cardGroupId;
        }

        public CardGroupId CardGroupId { get; }
    }

    public static class ShowDeckListRequestExtensions {
        public static void PublishShowDeckListRequest(this IEventBus eventBus, CardGroupId cardGroupId) {
            eventBus.Publish(new ShowDeckListRequest(cardGroupId));
        }

        public static void SubscribeToShowDeckListRequest(this IEventBus eventBus, Action<ShowDeckListRequest> callback) {
            eventBus.Subscribe(callback);
        }

        public static void UnsubscribeFromShowDeckListRequest(this IEventBus eventBus, Action<ShowDeckListRequest> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
