using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Events {
    public class ButtonTextChangedEvent : ICrossAppEvent, IButtonContext {
        public ButtonTextChangedEvent(CardGroup cardGroup, int cardZoneIndex, int index, string text) {
            CardGroup = cardGroup;
            CardZoneIndex = cardZoneIndex;
            Index = index;
            Text = text;
        }

        public CardGroup CardGroup { get; }
        public int CardZoneIndex { get; }
        public int Index { get; }
        public string Text;
}

    public static class ButtonTextChangedEventExtensions {
        public static void PublishButtonTextChangedEvent(this IEventBus eventBus, CardGroup cardGroup, int cardZoneIndex, int index, string text) {
            eventBus.Publish(new ButtonTextChangedEvent(cardGroup, cardZoneIndex, index, text));
        }

        public static void SubscribeToButtonTextChangedEvent(this IEventBus eventBus, Action<ButtonTextChangedEvent> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
