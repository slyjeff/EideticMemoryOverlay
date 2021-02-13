using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class DynamicButtonClickRequest : IEvent, IButtonContext {
        public DynamicButtonClickRequest(CardGroupId cardGroupId, ButtonMode buttonMode, int index, bool isLeftClick) {
            CardGroupId = cardGroupId;
            ButtonMode = buttonMode;
            Index = index;
            IsLeftClick = isLeftClick;
        }

        public CardGroupId CardGroupId { get; }
        public ButtonMode ButtonMode { get; }
        public int Index { get; }
        public bool IsLeftClick { get; }
    }

    public static class DynamicButtonClickExtensions {
        public static void PublishDynamicButtonClickRequest(this IEventBus eventBus, CardGroupId cardGroupId, ButtonMode buttonMode, int index, bool isLeftClick) {
            eventBus.Publish(new DynamicButtonClickRequest(cardGroupId, buttonMode, index, isLeftClick));
        }

        public static void SubscribeToDynamicButtonClickRequest(this IEventBus eventBus, Action<DynamicButtonClickRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
