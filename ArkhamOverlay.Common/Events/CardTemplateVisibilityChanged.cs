using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Events {
    public class CardInfoVisibilityChanged : ICrossAppEvent {
        public CardInfoVisibilityChanged(string name, bool isVisible) {
            Name = name;
            IsVisible = isVisible;
        }

        public string Name;
        public bool IsVisible;
    }

    public static class CardInfoVisibilityChangedExtensions {
        public static void PublishCardInfoVisibilityChanged(this IEventBus eventBus, string name, bool isVisible) {
            eventBus.Publish(new CardInfoVisibilityChanged(name, isVisible));
        }

        public static void SubscribeToCardInfoVisibilityChanged(this IEventBus eventBus, Action<CardInfoVisibilityChanged> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
