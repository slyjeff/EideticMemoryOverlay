using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using System;

namespace ArkhamOverlay.Events {
    public enum MouseButton { Left, Right }

    public class ButtonClickRequest : ICrossAppEvent, IButtonContext {
        public ButtonClickRequest(CardGroupId cardGroupId, ButtonMode buttonMode, int index, MouseButton mouseButton, string selectedOption) {
            CardGroupId = cardGroupId;
            ButtonMode = buttonMode;
            Index = index;
            MouseButton = mouseButton;
            SelectedOption = selectedOption;
        }

        public CardGroupId CardGroupId { get; }
        public ButtonMode ButtonMode { get; }
        public int Index { get; }
        public MouseButton MouseButton { get; }
        public string SelectedOption { get; }
    }

    public static class ButtonClickRequestExtensions {
        public static void PublishButtonClickRequest(this IEventBus eventBus, CardGroupId cardGroupId, ButtonMode buttonMode, int index, MouseButton mouseButton, string seletedOption) {
            eventBus.Publish(new ButtonClickRequest(cardGroupId, buttonMode, index, mouseButton, seletedOption));
        }

        public static void SubscribeToButtonClickRequest(this IEventBus eventBus, Action<ButtonClickRequest> callback) {
            eventBus.Subscribe(callback);
        }
        public static void UnsubscribeFromButtonClickRequest(this IEventBus eventBus, Action<ButtonClickRequest> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
