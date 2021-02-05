using ArkhamOverlay.TcpUtils;
using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class ChangeStatValue : IEvent {
        public ChangeStatValue(Deck deck, StatType statType, bool increase) {
            Deck = deck;
            StatType = statType;
            Increase = increase;
        }

        public Deck Deck { get; }
        public StatType StatType { get; }
        public bool Increase { get; }
    }

    public static class ChangeStatValueExtensions {
        public static void ChangeStatValue(this IEventBus eventBus, Deck deck, StatType statType, bool increase) {
            eventBus.Publish(new ChangeStatValue(deck, statType, increase));
        }

        public static void OnChangeStatValue(this IEventBus eventBus, Action<Deck, StatType, bool> action) {
            eventBus.Subscribe<ChangeStatValue>(x => action?.Invoke(x.Deck, x.StatType, x.Increase));
        }
    }
}
