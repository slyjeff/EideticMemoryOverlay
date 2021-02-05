using ArkhamOverlay.TcpUtils;
using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class ShowDeckList : IEvent {
        public ShowDeckList(Deck deck) {
            Deck = deck;
        }

        public Deck Deck { get; }
    }

    public static class ShowDeckListExtensions {
        public static void ShowDeckList(this IEventBus eventBus, Deck deck) {
            eventBus.Publish(new ShowDeckList(deck));
        }

        public static void OnShowDeckList(this IEventBus eventBus, Action<Deck> action) {
            eventBus.Subscribe<ShowDeckList>(x => action?.Invoke(x.Deck));
        }
    }
}
