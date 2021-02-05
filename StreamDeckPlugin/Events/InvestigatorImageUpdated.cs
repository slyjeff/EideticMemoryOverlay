using ArkhamOverlay.TcpUtils;
using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class InvestigatorImageUpdated : IEvent {
        public InvestigatorImageUpdated(Deck deck, byte[] bytes) {
            Deck = deck;
            Bytes = bytes;
        }

        public Deck Deck { get; }
        public byte[] Bytes { get; }
    }

    public static class InvestigatorImageUpdatedExtensions {
        public static void UpdateInvestigatorImage(this IEventBus eventBus, Deck deck, byte[] bytes) {
            eventBus.Publish(new InvestigatorImageUpdated(deck, bytes));
        }

        public static void OnInvestigatorImageUpdated(this IEventBus eventBus, Action<Deck, byte[]> action) {
            eventBus.Subscribe<InvestigatorImageUpdated>(x => action?.Invoke(x.Deck, x.Bytes));
        }
    }
}
