using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Common.Events {
    public class StatUpdated : ICrossAppEvent {
        public StatUpdated(CardGroup deck, StatType statType, int value) {
            Deck = deck;
            StatType = statType;
            Value = value;
        }

        public CardGroup Deck { get; }
        public StatType StatType { get; }
        public int Value { get; }
    }

    public static class StatUpdatedExtensions {
        public static void PublishStatUpdated(this IEventBus eventBus, CardGroup deck, StatType statType, int value) {
            eventBus.Publish(new StatUpdated(deck, statType, value));
        }

        public static void SubscribeToStatUpdated(this IEventBus eventBus, Action<StatUpdated> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
