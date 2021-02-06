using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class GetButtonImageRequest : IEvent {
        public GetButtonImageRequest(Deck deck, int index, DynamicActionMode mode) {
            Deck = deck;
            Index = index;
            Mode = mode;
        }

        public Deck Deck { get; }
        public int Index { get; }
        public DynamicActionMode Mode { get; }
    }

    public static class GetButtonImageRequestExtensions {
        public static void PublishGetButtonImageRequest(this IEventBus eventBus, Deck deck, int index, DynamicActionMode mode) {
            eventBus.Publish(new GetButtonImageRequest(deck, index, mode));
        }

        public static void SubscribeToGetButtonImageRequest(this IEventBus eventBus, Action<GetButtonImageRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
