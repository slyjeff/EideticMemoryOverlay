using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Common.Events {
    public class ShowDeckListRequest : ICrossAppEvent {
        public ShowDeckListRequest(CardGroup deck) {
            Deck = deck;
        }

        public CardGroup Deck { get; }
    }

    public static class ShowDeckListRequestExtensions {
        public static void PublishShowDeckListRequest(this IEventBus eventBus, CardGroup deck) {
            eventBus.Publish(new ShowDeckListRequest(deck));
        }

        public static void SubscribeToShowDeckListRequest(this IEventBus eventBus, Action<ShowDeckListRequest> callback) {
            eventBus.Subscribe(callback);
        }

        public static void UnsubscribeFromShowDeckListRequest(this IEventBus eventBus, Action<ShowDeckListRequest> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
