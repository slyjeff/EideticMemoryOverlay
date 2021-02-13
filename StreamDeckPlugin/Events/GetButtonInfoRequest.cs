using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class GetButtonInfoRequest : IEvent, IButtonContext {
        public GetButtonInfoRequest(CardGroupId cardGroup, int cardZoneIndex, int index) {
            CardGroup = cardGroup;
            CardZoneIndex = cardZoneIndex;
            Index = index;
        }

        public CardGroupId CardGroup { get; }
        public int CardZoneIndex { get; }
        public int Index { get; }
    }

    public static class GetButtonInfoRequestExtensions {
        public static void PublishGetButtonInfoRequest(this IEventBus eventBus, CardGroupId cardGroup, int cardZoneIndex, int index) {
            eventBus.Publish(new GetButtonInfoRequest(cardGroup, cardZoneIndex, index));
        }

        public static void SubscribeToGetButtonInfoRequest(this IEventBus eventBus, Action<GetButtonInfoRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
