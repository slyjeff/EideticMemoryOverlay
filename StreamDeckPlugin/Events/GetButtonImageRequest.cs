using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class GetButtonImageRequest : IEvent, IButtonContext {
        public GetButtonImageRequest(CardGroup cardGroup, int cardZoneIndex, int index) {
            CardGroup = cardGroup;
            CardZoneIndex = cardZoneIndex;
            Index = index;
        }

        public CardGroup CardGroup { get; }
        public int CardZoneIndex { get; }
        public int Index { get; }
    }

    public static class GetButtonImageRequestExtensions {
        public static void PublishGetButtonImageRequest(this IEventBus eventBus, CardGroup cardGroup, int cardZoneIndex, int index) {
            eventBus.Publish(new GetButtonImageRequest(cardGroup, cardZoneIndex, index));
        }

        public static void SubscribeToGetButtonImageRequest(this IEventBus eventBus, Action<GetButtonImageRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
