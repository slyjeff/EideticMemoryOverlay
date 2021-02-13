using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Common.Events {
    public class StatUpdated : ICrossAppEvent {
        public StatUpdated(CardGroupId deck, StatType statType, int value) {
            Deck = deck;
            StatType = statType;
            Value = value;
        }

        public CardGroupId Deck { get; }
        public StatType StatType { get; }
        public int Value { get; }
    }

    public static class StatUpdatedExtensions {
        public static void PublishStatUpdated(this IEventBus eventBus, CardGroupId deck, StatType statType, int value) {
            eventBus.Publish(new StatUpdated(deck, statType, value));
        }

        public static void SubscribeToStatUpdated(this IEventBus eventBus, Action<StatUpdated> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
