using Emo.Common.Enums;
using Emo.Common.Services;
using Emo.Common.Utils;
using System;

namespace StreamDeckPlugin.Events {
    public class GetButtonImageRequest : IEvent {
        public GetButtonImageRequest(CardGroupId cardGroup, ButtonMode? buttonMode, int? zoneIndex, int? index) {
            CardGroupId = cardGroup;
            ButtonMode = buttonMode;
            ZoneIndex = zoneIndex;
            Index = index;
        }

        public CardGroupId CardGroupId { get; }
        public ButtonMode? ButtonMode { get; }
        public int? ZoneIndex { get; }
        public int? Index { get; }
    }

    public static class GetButtonImageRequestExtensions {
        public static void PublishGetButtonImageRequest(this IEventBus eventBus, CardGroupId cardGroup, ButtonMode? buttonMode, int? zoneIndex, int? index) {
            eventBus.Publish(new GetButtonImageRequest(cardGroup, buttonMode, zoneIndex, index));
        }

        public static void SubscribeToGetButtonImageRequest(this IEventBus eventBus, Action<GetButtonImageRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
