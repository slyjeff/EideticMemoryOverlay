using Emo.Common.Enums;
using Emo.Common.Services;
using Emo.Common.Utils;
using System;

namespace Emo.Events {
    public class ButtonToggled : ICrossAppEvent, IButtonContext {
        public ButtonToggled(CardGroupId cardGroupId, ButtonMode buttonMode, int zoneIndex, int index, bool isToggled) {
            CardGroupId = cardGroupId;
            ButtonMode = buttonMode;
            ZoneIndex = zoneIndex;
            Index = index;
            IsToggled = isToggled;
        }

        public CardGroupId CardGroupId { get; }
        public ButtonMode ButtonMode { get; }
        public int ZoneIndex { get; }
        public int Index { get; }
        public bool IsToggled { get; }
    }

    public static class ButtonToggledExtensions {
        public static void PublishButtonToggled(this IEventBus eventBus, CardGroupId cardGroupId, ButtonMode buttonMode, int zoneIndex, int index, bool isToggled) {
            eventBus.Publish(new ButtonToggled(cardGroupId, buttonMode, zoneIndex, index, isToggled));
        }

        public static void SubscribeToButtonToggled(this IEventBus eventBus, Action<ButtonToggled> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
