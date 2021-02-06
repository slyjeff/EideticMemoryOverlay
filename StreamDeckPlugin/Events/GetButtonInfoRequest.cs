using ArkhamOverlay.Common.Enums;
using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class GetButtonInfoRequest : IEvent {
        public GetButtonInfoRequest(Deck deck, int index, DynamicActionMode mode) {
            Deck = deck;
            Index = index;
            Mode = mode;
        }

        public Deck Deck { get; }
        public int Index { get; }
        public DynamicActionMode Mode { get; }
    }

    public static class GetButtonInfoRequestExtensions {
        public static void PublishGetButtonInfoRequest(this IEventBus eventBus, Deck deck, int index, DynamicActionMode mode) {
            eventBus.Publish(new GetButtonInfoRequest(deck, index, mode));
        }

        public static void SubscribeToGetButtonInfoRequest(this IEventBus eventBus, Action<GetButtonInfoRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
