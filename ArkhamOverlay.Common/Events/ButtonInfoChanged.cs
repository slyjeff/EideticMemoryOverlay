using ArkhamOverlay.Common;
using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Events {
    public class ButtonInfoChanged : ICrossAppEvent, IButtonContext, ICardInfo {
        public ButtonInfoChanged(CardGroup cardGroup, int cardZoneIndex, int index, string name, bool isToggled, bool imageAvailable) {
            CardGroup = cardGroup;
            CardZoneIndex = cardZoneIndex;
            Index = index;
            Name = name;
            IsToggled = isToggled;
            ImageAvailable = imageAvailable;
        }

        public CardGroup CardGroup { get; }
        public int CardZoneIndex { get; }
        public int Index { get; }
        public string Name { get; }
        public bool IsToggled { get; }
        public bool ImageAvailable { get; }
    }

    public static class ButtonInfoChangedExtensions {
        public static void PublishButtonInfoChanged(this IEventBus eventBus, CardGroup cardGroup, int cardZoneIndex, int index, string name, bool isToggled, bool imageAvailable) {
            eventBus.Publish(new ButtonInfoChanged(cardGroup, cardZoneIndex, index, name, isToggled, imageAvailable));
        }

        public static void SubscribeToButtonInfoChanged(this IEventBus eventBus, Action<ButtonInfoChanged> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
