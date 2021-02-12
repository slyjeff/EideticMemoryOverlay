using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class GetStatValueRequest : IEvent {
        public GetStatValueRequest(CardGroup deck, StatType statType) {
            Deck = deck;
            StatType = statType;
        }

        public CardGroup Deck { get; }
        public StatType StatType { get; }
    }

    public static class GetStatValueRequestExtensions {
        public static void PublishGetStatValueRequest(this IEventBus eventBus, CardGroup deck, StatType statType) {
            eventBus.Publish(new GetStatValueRequest(deck, statType));
        }

        public static void SubscribeToGetStatValueRequest(this IEventBus eventBus, Action<GetStatValueRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
