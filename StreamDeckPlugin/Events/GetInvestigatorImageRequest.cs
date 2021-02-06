using ArkhamOverlay.Common.Enums;
using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class GetInvestigatorImageRequest : IEvent {
        public GetInvestigatorImageRequest(Deck deck) {
            Deck = deck;
        }

        public Deck Deck { get; }
    }

    public static class GetInvestigatorImageRequestExtensions {
        public static void PublishGetInvestigatorImageRequest(this IEventBus eventBus, Deck deck) {
            eventBus.Publish(new GetInvestigatorImageRequest(deck));
        }

        public static void SubscribeToGetInvestigatorImageRequest(this IEventBus eventBus, Action<GetInvestigatorImageRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
