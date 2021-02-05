using ArkhamOverlay.TcpUtils;
using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class GetInvestigatorImage : IEvent {
        public GetInvestigatorImage(Deck deck) {
            Deck = deck;
        }

        public Deck Deck { get; }
    }

    public static class GetInvestigatorImageExtensions {
        public static void GetInvestigatorImage(this IEventBus eventBus, Deck deck) {
            eventBus.Publish(new GetInvestigatorImage(deck));
        }

        public static void OnGetInvestigatorImage(this IEventBus eventBus, Action<Deck> action) {
            eventBus.Subscribe<GetInvestigatorImage>(x => action?.Invoke(x.Deck));
        }
    }
}
