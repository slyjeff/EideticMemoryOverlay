using ArkhamOverlay.TcpUtils;
using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class DynamicButtonClickRequest : IEvent {
        public DynamicButtonClickRequest(Deck deck, int index, DynamicActionMode mode, bool isLeftClick) {
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
        public static void PublicDynamicButtonClickRequest(this IEventBus eventBus, Deck deck, int index, DynamicActionMode mode, bool isLeftClick) {
            eventBus.Publish(new DynamicButtonClickRequest(deck, index, mode, isLeftClick));
        }

        public static void SubscribeToDynamicButtonClickRequest(this IEventBus eventBus, Action<DynamicButtonClickRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
