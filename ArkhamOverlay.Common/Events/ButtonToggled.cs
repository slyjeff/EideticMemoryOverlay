using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Events {
    public class ButtonToggled : ICrossAppEvent, IButtonContext {
        public ButtonToggled(CardGroupId cardGroup, ButtonMode buttonMode, int index, bool isToggled) {
            CardGroupId = cardGroup;
            ButtonMode = buttonMode;
            Index = index;
            IsToggled = isToggled;
        }

        public CardGroupId CardGroupId { get; }
        public ButtonMode ButtonMode { get; }
        public int Index { get; }

        public bool IsToggled;
    }

    public static class ButtonToggledExtensions {
        public static void PublishButtonToggled(this IEventBus eventBus, CardGroupId cardGroup, ButtonMode buttonMode, int index, bool isToggled) {
            eventBus.Publish(new ButtonToggled(cardGroup, buttonMode, index, isToggled));
        }

        public static void SubscribeToButtonToggled(this IEventBus eventBus, Action<ButtonToggled> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
