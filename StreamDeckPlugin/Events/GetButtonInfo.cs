using ArkhamOverlay.TcpUtils;
using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class GetButtonInfo : IEvent {
        public GetButtonInfo(Deck deck, int index, DynamicActionMode mode) {
            Deck = deck;
            Index = index;
            Mode = mode;
        }

        public Deck Deck { get; }
        public int Index { get; }
        public DynamicActionMode Mode { get; }
    }

    public static class GetButtonInfoExtensions {
        public static void GetButtonInfo(this IEventBus eventBus, Deck deck, int index, DynamicActionMode mode) {
            eventBus.Publish(new GetButtonInfo(deck, index, mode));
        }

        public static void OnGetButtonInfo(this IEventBus eventBus, Action<Deck, int, DynamicActionMode> action) {
            eventBus.Subscribe<GetButtonInfo>(x => action?.Invoke(x.Deck, x.Index, x.Mode));
        }
    }
}
