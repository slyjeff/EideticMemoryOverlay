using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Events {
    public class ButtonTextChanged : ICrossAppEvent, IButtonContext {
        public ButtonTextChanged(CardGroupId cardGroup, int cardZoneIndex, int index, string text) {
            CardGroup = cardGroup;
            CardZoneIndex = cardZoneIndex;
            Index = index;
            Text = text;
        }

        public CardGroupId CardGroup { get; }
        public int CardZoneIndex { get; }
        public int Index { get; }
        public string Text;
}

    public static class ButtonTextChangedExtensions {
        public static void PublishButtonTextChanged(this IEventBus eventBus, CardGroupId cardGroup, int cardZoneIndex, int index, string text) {
            eventBus.Publish(new ButtonTextChanged(cardGroup, cardZoneIndex, index, text));
        }

        public static void SubscribeToButtonTextChanged(this IEventBus eventBus, Action<ButtonTextChanged> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
