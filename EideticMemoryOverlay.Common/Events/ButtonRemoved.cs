using Emo.Common.Enums;
using Emo.Common.Services;
using Emo.Common.Utils;
using System;

namespace Emo.Events {
    public class ButtonRemoved : ICrossAppEvent, IButtonContext {
        public ButtonRemoved(CardGroupId cardGroupId, ButtonMode buttonMode, int zoneIndex, int index) {
            CardGroupId = cardGroupId;
            ButtonMode = buttonMode;
            ZoneIndex = zoneIndex;
            Index = index;
        }

        public CardGroupId CardGroupId { get; }
        public ButtonMode ButtonMode { get; }
        public int ZoneIndex { get; }
        public int Index { get; }
    }

    public static class ButtonRemovedExtensions {
        public static void PublishButtonRemoved(this IEventBus eventBus, CardGroupId cardGroupId, ButtonMode buttonMode, int zoneIndex, int index) {
            eventBus.Publish(new ButtonRemoved(cardGroupId, buttonMode, zoneIndex, index));
        }

        public static void SubscribeToButtonRemoved(this IEventBus eventBus, Action<ButtonRemoved> callback) {
            eventBus.Subscribe(callback);
        }

        public static void UnsubscribeFromButtonRemoved(this IEventBus eventBus, Action<ButtonRemoved> callback) {
            eventBus.Unsubscribe(callback);
        }

    }
}
