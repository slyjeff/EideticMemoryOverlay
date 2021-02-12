using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Common.Events {
    public class StatUpdatedEvent : ICrossAppEvent {
        public StatUpdatedEvent(CardGroup deck, StatType statType, int value) {
            Deck = deck;
            StatType = statType;
            Value = value;
        }

        public CardGroup Deck { get; }
        public StatType StatType { get; }
        public int Value { get; }
    }

    public static class StatUpdatedEventExtensions {
        public static void PublishStatUpdatedEvent(this IEventBus eventBus, CardGroup deck, StatType statType, int value) {
            eventBus.Publish(new StatUpdatedEvent(deck, statType, value));
        }

        public static void SubscribeToStatUpdatedEvent(this IEventBus eventBus, Action<StatUpdatedEvent> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
