using Emo.Common.Enums;
using Emo.Common.Services;
using Emo.Common.Utils;
using System;

namespace StreamDeckPlugin.Events {
    public class GetButtonInfoRequest : IEvent, IButtonContext {
        public GetButtonInfoRequest(CardGroupId cardGroup, ButtonMode buttonMode, int zoneIndex, int index) {
            CardGroupId = cardGroup;
            ButtonMode = buttonMode;
            ZoneIndex = zoneIndex;
            Index = index;
        }

        public CardGroupId CardGroupId { get; }
        public ButtonMode ButtonMode { get; }
        public int ZoneIndex { get; }
        public int Index { get; }
    }

    public static class GetButtonInfoRequestExtensions {
        public static void PublishGetButtonInfoRequest(this IEventBus eventBus, CardGroupId cardGroup, ButtonMode buttonMode, int zoneIndex, int index) {
            eventBus.Publish(new GetButtonInfoRequest(cardGroup, buttonMode, zoneIndex, index));
        }

        public static void SubscribeToGetButtonInfoRequest(this IEventBus eventBus, Action<GetButtonInfoRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
