using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Common.Events {
    public class StatUpdated : ICrossAppEvent {
        public StatUpdated(CardGroupId cardGroupId, StatType statType, int value) {
            CardGroupId = cardGroupId;
            StatType = statType;
            Value = value;
        }

        public CardGroupId CardGroupId { get; }
        public StatType StatType { get; }
        public int Value { get; }
    }

    public static class StatUpdatedExtensions {
        public static void PublishStatUpdated(this IEventBus eventBus, CardGroupId cardGroupId, StatType statType, int value) {
            eventBus.Publish(new StatUpdated(cardGroupId, statType, value));
        }

        public static void SubscribeToStatUpdated(this IEventBus eventBus, Action<StatUpdated> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
