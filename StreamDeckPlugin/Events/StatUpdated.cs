using ArkhamOverlay.TcpUtils;
using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class StatUpdated : IEvent {
        public StatUpdated(Deck deck, StatType statType, int value) {
            Deck = deck;
            StatType = statType;
            Value = value;
        }

        public Deck Deck { get; }
        public StatType StatType { get; }
        public int Value { get; }
    }

    public static class StatUpdatedExtensions {
        public static void StatUpdated(this IEventBus eventBus, Deck deck, StatType statType, int value) {
            eventBus.Publish(new StatUpdated(deck, statType, value));
        }

        public static void OnStatUpdated(this IEventBus eventBus, Action<Deck, StatType, int> action) {
            eventBus.Subscribe<StatUpdated>(x => action?.Invoke(x.Deck, x.StatType, x.Value));
        }
    }
}
