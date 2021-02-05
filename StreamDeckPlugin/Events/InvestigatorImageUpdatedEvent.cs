using ArkhamOverlay.TcpUtils;
using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class InvestigatorImageUpdatedEvent : IEvent {
        public InvestigatorImageUpdatedEvent(Deck deck, byte[] bytes) {
            Deck = deck;
            Bytes = bytes;
        }

        public Deck Deck { get; }
        public byte[] Bytes { get; }
    }

    public static class InvestigatorImageUpdatedEventExtensions {
        public static void PublishInvestigatorImageUpdatedEvent(this IEventBus eventBus, Deck deck, byte[] bytes) {
            eventBus.Publish(new InvestigatorImageUpdatedEvent(deck, bytes));
        }

        public static void SubscribeToInvestigatorImageUpdatedEvent(this IEventBus eventBus, Action<InvestigatorImageUpdatedEvent> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
