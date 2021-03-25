using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using System;
using System.Collections.Generic;

namespace ArkhamOverlay.Events {
    public class CardGroupButtonsChanged : ICrossAppEvent {
        public CardGroupButtonsChanged(CardGroupId cardGroupId, IList<ButtonInfo> buttons) {
            CardGroupId = cardGroupId;
            Buttons = buttons;
        }

        public CardGroupId CardGroupId { get; }
        public IList<ButtonInfo> Buttons { get; }
    }

    public static class CardGroupButtonsChangedExtensions {
        public static void PublishCardGroupButtonsChanged(this IEventBus eventBus, CardGroupId cardGroupId, IList<ButtonInfo> buttons) {
            eventBus.Publish(new CardGroupButtonsChanged(cardGroupId, buttons));
        }

        public static void SubscribeToCardGroupButtonsChanged(this IEventBus eventBus, Action<CardGroupButtonsChanged> callback) {
            eventBus.Subscribe(callback);
        }
        public static void UnsubscribeFromCardGroupButtonsChanged(this IEventBus eventBus, Action<CardGroupButtonsChanged> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
