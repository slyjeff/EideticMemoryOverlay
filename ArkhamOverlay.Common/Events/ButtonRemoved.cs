using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using System;

namespace ArkhamOverlay.Events {
    public class ButtonRemoved : ICrossAppEvent, IButtonContext {
        public ButtonRemoved(CardGroupId cardGroupId, ButtonMode buttonMode, int index) {
            CardGroupId = cardGroupId;
            ButtonMode = buttonMode;
            Index = index;
        }

        public CardGroupId CardGroupId { get; }
        public ButtonMode ButtonMode { get; }
        public int Index { get; }
    }

    public static class ButtonRemovedExtensions {
        public static void PublishButtonRemoved(this IEventBus eventBus, CardGroupId cardGroupId, ButtonMode buttonMode, int index) {
            eventBus.Publish(new ButtonRemoved(cardGroupId, buttonMode, index));
        }

        public static void SubscribeToButtonRemoved(this IEventBus eventBus, Action<ButtonRemoved> callback) {
            eventBus.Subscribe(callback);
        }

        public static void UnsubscribeFromButtonRemoved(this IEventBus eventBus, Action<ButtonRemoved> callback) {
            eventBus.Unsubscribe(callback);
        }

    }
}
