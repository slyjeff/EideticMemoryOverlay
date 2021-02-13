using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Events {
    public class ButtonTextChanged : ICrossAppEvent, IButtonContext {
        public ButtonTextChanged(CardGroupId cardGroup, ButtonMode buttonMode, int index, string text) {
            CardGroupId = cardGroup;
            Index = index;
            Text = text;
        }

        public CardGroupId CardGroupId { get; }
        public ButtonMode ButtonMode { get; }
        public int Index { get; }
        public string Text { get; }
}

    public static class ButtonTextChangedExtensions {
        public static void PublishButtonTextChanged(this IEventBus eventBus, CardGroupId cardGroup, ButtonMode buttonMode, int index, string text) {
            eventBus.Publish(new ButtonTextChanged(cardGroup, buttonMode, index, text));
        }

        public static void SubscribeToButtonTextChanged(this IEventBus eventBus, Action<ButtonTextChanged> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
