using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class InvestigatorImageUpdatedEvent : IEvent {
        public InvestigatorImageUpdatedEvent(CardGroupId cardGroup, byte[] bytes) {
            CardGroup = cardGroup;
            Bytes = bytes;
        }

        public CardGroupId CardGroup { get; }
        public byte[] Bytes { get; }
    }

    public static class InvestigatorImageUpdatedEventExtensions {
        public static void PublishInvestigatorImageUpdatedEvent(this IEventBus eventBus, CardGroupId cardGroup, byte[] bytes) {
            eventBus.Publish(new InvestigatorImageUpdatedEvent(cardGroup, bytes));
        }

        public static void SubscribeToInvestigatorImageUpdatedEvent(this IEventBus eventBus, Action<InvestigatorImageUpdatedEvent> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
