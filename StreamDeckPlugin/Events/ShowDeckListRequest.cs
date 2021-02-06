using ArkhamOverlay.Common.Enums;
using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class ShowDeckListRequest : IEvent {
        public ShowDeckListRequest(Deck deck) {
            Deck = deck;
        }

        public Deck Deck { get; }
    }

    public static class ShowDeckListRequestExtensions {
        public static void PublishShowDeckListRequest(this IEventBus eventBus, Deck deck) {
            eventBus.Publish(new ShowDeckListRequest(deck));
        }

        public static void SubscribeToShowDeckListRequest(this IEventBus eventBus, Action<ShowDeckListRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
