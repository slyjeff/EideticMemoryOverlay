using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using System;

namespace ArkhamOverlay.Events {
    public class ButtonToggled : ICrossAppEvent, IButtonContext {
        public ButtonToggled(CardGroupId cardGroupId, ButtonMode buttonMode, int index, bool isToggled) {
            CardGroupId = cardGroupId;
            ButtonMode = buttonMode;
            Index = index;
            IsToggled = isToggled;
        }

        public CardGroupId CardGroupId { get; }
        public ButtonMode ButtonMode { get; }
        public int Index { get; }
        public bool IsToggled { get; }
    }

    public static class ButtonToggledExtensions {
        public static void PublishButtonToggled(this IEventBus eventBus, CardGroupId cardGroupId, ButtonMode buttonMode, int index, bool isToggled) {
            eventBus.Publish(new ButtonToggled(cardGroupId, buttonMode, index, isToggled));
        }

        public static void SubscribeToButtonToggled(this IEventBus eventBus, Action<ButtonToggled> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
