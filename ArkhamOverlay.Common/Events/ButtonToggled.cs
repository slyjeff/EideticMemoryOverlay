using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Events {
    public class ButtonToggled : ICrossAppEvent, IButtonContext {
        public ButtonToggled(CardGroupId cardGroup, int cardZoneIndex, int index, bool isToggled) {
            CardGroup = cardGroup;
            CardZoneIndex = cardZoneIndex;
            Index = index;
            IsToggled = isToggled;
        }

        public CardGroupId CardGroup { get; }
        public int CardZoneIndex { get; }
        public int Index { get; }

        public bool IsToggled;
    }

    public static class ButtonToggledExtensions {
        public static void PublishButtonToggled(this IEventBus eventBus, CardGroupId cardGroup, int cardZoneIndex, int index, bool isToggled) {
            eventBus.Publish(new ButtonToggled(cardGroup, cardZoneIndex, index, isToggled));
        }

        public static void SubscribeToButtonToggled(this IEventBus eventBus, Action<ButtonToggled> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
