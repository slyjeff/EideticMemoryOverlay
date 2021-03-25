using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using System;

namespace ArkhamOverlay.Events {
    public class ButtonTextChanged : ICrossAppEvent, IButtonContext {
        public ButtonTextChanged(CardGroupId cardGroupId, ButtonMode buttonMode, int zoneIndex, int index, string text) {
            CardGroupId = cardGroupId;
            ButtonMode = buttonMode;
            ZoneIndex = zoneIndex;
            Index = index;
            Text = text;
        }

        public CardGroupId CardGroupId { get; }
        public ButtonMode ButtonMode { get; }
        public int ZoneIndex { get; }
        public int Index { get; }
        public string Text { get; }
}

    public static class ButtonTextChangedExtensions {
        public static void PublishButtonTextChanged(this IEventBus eventBus, CardGroupId cardGroupId, ButtonMode buttonMode, int zoneIndex, int index, string text) {
            eventBus.Publish(new ButtonTextChanged(cardGroupId, buttonMode, zoneIndex, index, text));
        }

        public static void SubscribeToButtonTextChanged(this IEventBus eventBus, Action<ButtonTextChanged> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
