using ArkhamOverlay.TcpUtils;
using StreamDeckPlugin.Services;
using StreamDeckPlugin.Utils;
using System;

namespace StreamDeckPlugin.Events {
    public class GetButtonImage : IEvent {
        public GetButtonImage(Deck deck, int index, DynamicActionMode mode) {
            Deck = deck;
            Index = index;
            Mode = mode;
        }

        public Deck Deck { get; }
        public int Index { get; }
        public DynamicActionMode Mode { get; }
    }

    public static class GetButtonImageExtensions {
        public static void GetButtonImage(this IEventBus eventBus, Deck deck, int index, DynamicActionMode mode) {
            eventBus.Publish(new GetButtonImage(deck, index, mode));
        }

        public static void OnGetButtonImage(this IEventBus eventBus, Action<Deck, int, DynamicActionMode> action) {
            eventBus.Subscribe<GetButtonImage>(x => action?.Invoke(x.Deck, x.Index, x.Mode));
        }
    }
}
