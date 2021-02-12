using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Events {
    public class ButtonToggledEvent : ICrossAppEvent, IButtonContext {
        public ButtonToggledEvent(CardGroup cardGroup, int cardZoneIndex, int index, bool isToggled) {
            CardGroup = cardGroup;
            CardZoneIndex = cardZoneIndex;
            Index = index;
            IsToggled = isToggled;
        }

        public CardGroup CardGroup { get; }
        public int CardZoneIndex { get; }
        public int Index { get; }

        public bool IsToggled;
    }

    public static class ButtonToggledEventExtensions {
        public static void PublishButtonToggledEvent(this IEventBus eventBus, CardGroup cardGroup, int cardZoneIndex, int index, bool isToggled) {
            eventBus.Publish(new ButtonToggledEvent(cardGroup, cardZoneIndex, index, isToggled));
        }

        public static void SubscribeToButtonToggledEvent(this IEventBus eventBus, Action<ButtonToggledEvent> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
