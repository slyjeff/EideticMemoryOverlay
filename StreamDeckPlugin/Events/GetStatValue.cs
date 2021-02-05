using ArkhamOverlay.TcpUtils;
using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class GetStatValue : IEvent {
        public GetStatValue(Deck deck, StatType statType) {
            Deck = deck;
            StatType = statType;
        }

        public Deck Deck { get; }
        public StatType StatType { get; }
    }

    public static class GetStatValueExtensions {
        public static void GetStatValue(this IEventBus eventBus, Deck deck, StatType statType) {
            eventBus.Publish(new GetStatValue(deck, statType));
        }

        public static void OnGetStatValue(this IEventBus eventBus, Action<Deck, StatType> action) {
            eventBus.Subscribe<GetStatValue>(x => action?.Invoke(x.Deck, x.StatType));
        }
    }
}
