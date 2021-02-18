using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class GetStatValueRequest : IEvent {
        public GetStatValueRequest(CardGroupId deck, StatType statType) {
            Deck = deck;
            StatType = statType;
        }

        public CardGroupId Deck { get; }
        public StatType StatType { get; }
    }

    public static class GetStatValueRequestExtensions {
        public static void PublishGetStatValueRequest(this IEventBus eventBus, CardGroupId deck, StatType statType) {
            eventBus.Publish(new GetStatValueRequest(deck, statType));
        }

        public static void SubscribeToGetStatValueRequest(this IEventBus eventBus, Action<GetStatValueRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
