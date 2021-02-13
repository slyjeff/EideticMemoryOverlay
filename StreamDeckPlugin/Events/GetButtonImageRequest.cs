using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class GetButtonImageRequest : IEvent, IButtonContext {
        public GetButtonImageRequest(CardGroupId cardGroup, int cardZoneIndex, int index) {
            CardGroup = cardGroup;
            CardZoneIndex = cardZoneIndex;
            Index = index;
        }

        public CardGroupId CardGroup { get; }
        public int CardZoneIndex { get; }
        public int Index { get; }
    }

    public static class GetButtonImageRequestExtensions {
        public static void PublishGetButtonImageRequest(this IEventBus eventBus, CardGroupId cardGroup, int cardZoneIndex, int index) {
            eventBus.Publish(new GetButtonImageRequest(cardGroup, cardZoneIndex, index));
        }

        public static void SubscribeToGetButtonImageRequest(this IEventBus eventBus, Action<GetButtonImageRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
