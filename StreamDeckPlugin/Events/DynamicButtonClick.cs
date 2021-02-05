using ArkhamOverlay.TcpUtils;
using StreamDeckPlugin.Services;
using StreamDeckPlugin.Utils;
using System;

namespace StreamDeckPlugin.Events {
    public class DynamicButtonClick : IEvent {
        public DynamicButtonClick(Deck deck, int index, DynamicActionMode mode, bool isLeftClick) {
            Deck = deck;
            Index = index;
            Mode = mode;
            IsLeftClick = isLeftClick;
        }

        public Deck Deck { get; }
        public int Index { get; }
        public DynamicActionMode Mode { get; }
        public bool IsLeftClick { get; }
    }

    public static class DynamicButtonClickExtensions {
        public static void DynamicButtonClick(this IEventBus eventBus, Deck deck, int index, DynamicActionMode mode, bool isLeftClick) {
            eventBus.Publish(new DynamicButtonClick(deck, index, mode, isLeftClick));
        }

        public static void OnDynamicButtonClicked(this IEventBus eventBus, Action<Deck, int, DynamicActionMode, bool> action) {
            eventBus.Subscribe<DynamicButtonClick>(x => action?.Invoke(x.Deck, x.Index, x.Mode, x.IsLeftClick));
        }
    }
}
