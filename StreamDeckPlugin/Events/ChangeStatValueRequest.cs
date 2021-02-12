using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class ChangeStatValueRequest : IEvent {
        public ChangeStatValueRequest(CardGroup deck, StatType statType, bool increase) {
            Deck = deck;
            StatType = statType;
            Increase = increase;
        }

        public CardGroup Deck { get; }
        public StatType StatType { get; }
        public bool Increase { get; }
    }

    public static class ChangeStatValueRequestExtensions {
        public static void PublishChangeStatValueRequest(this IEventBus eventBus, CardGroup deck, StatType statType, bool increase) {
            eventBus.Publish(new ChangeStatValueRequest(deck, statType, increase));
        }

        public static void SubscribeToStatValueRequest(this IEventBus eventBus, Action<ChangeStatValueRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
