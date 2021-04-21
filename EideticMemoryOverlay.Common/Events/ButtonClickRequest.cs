using Emo.Common.Enums;
using Emo.Common.Services;
using Emo.Common.Utils;
using System;

namespace Emo.Events {
    public enum MouseButton { Left, Right }

    public class ButtonClickRequest : ICrossAppEvent, IButtonContext {
        public ButtonClickRequest(CardGroupId cardGroupId, ButtonMode buttonMode, int zoneIndex, int index, MouseButton mouseButton, ButtonOption buttonOption) {
            CardGroupId = cardGroupId;
            ButtonMode = buttonMode;
            ZoneIndex = zoneIndex;
            Index = index;
            MouseButton = mouseButton;
            ButtonOption = buttonOption;
        }

        public CardGroupId CardGroupId { get; }
        public ButtonMode ButtonMode { get; }
        public int ZoneIndex { get; }
        public int Index { get; }
        public MouseButton MouseButton { get; }
        public ButtonOption ButtonOption{ get; }
    }

    public static class ButtonClickRequestExtensions {
        public static void PublishButtonClickRequest(this IEventBus eventBus, CardGroupId cardGroupId, ButtonMode buttonMode, int zoneIndex, int index, MouseButton mouseButton, ButtonOption buttonOption = null) {
            eventBus.Publish(new ButtonClickRequest(cardGroupId, buttonMode, zoneIndex, index, mouseButton, buttonOption));
        }

        public static void SubscribeToButtonClickRequest(this IEventBus eventBus, Action<ButtonClickRequest> callback) {
            eventBus.Subscribe(callback);
        }
        public static void UnsubscribeFromButtonClickRequest(this IEventBus eventBus, Action<ButtonClickRequest> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
