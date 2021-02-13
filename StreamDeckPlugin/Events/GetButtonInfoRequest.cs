using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class GetButtonInfoRequest : IEvent, IButtonContext {
        public GetButtonInfoRequest(CardGroupId cardGroup, ButtonMode buttonMode, int index) {
            CardGroupId = cardGroup;
            ButtonMode = buttonMode;
            Index = index;
        }

        public CardGroupId CardGroupId { get; }
        public ButtonMode ButtonMode { get; }
        public int Index { get; }
    }

    public static class GetButtonInfoRequestExtensions {
        public static void PublishGetButtonInfoRequest(this IEventBus eventBus, CardGroupId cardGroup, ButtonMode buttonMode, int index) {
            eventBus.Publish(new GetButtonInfoRequest(cardGroup, buttonMode, index));
        }

        public static void SubscribeToGetButtonInfoRequest(this IEventBus eventBus, Action<GetButtonInfoRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
