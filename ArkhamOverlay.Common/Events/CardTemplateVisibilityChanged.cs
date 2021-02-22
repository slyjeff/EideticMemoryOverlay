using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Events {
    public class CardInfoVisibilityChanged : ICrossAppEvent {
        public CardInfoVisibilityChanged(string code, bool isVisible) {
            Code = code;
            IsVisible = isVisible;
        }

        public string Code;
        public bool IsVisible;
    }

    public static class CardInfoVisibilityChangedExtensions {
        public static void PublishCardInfoVisibilityChanged(this IEventBus eventBus, string code, bool isVisible) {
            eventBus.Publish(new CardInfoVisibilityChanged(code, isVisible));
        }

        public static void SubscribeToCardInfoVisibilityChanged(this IEventBus eventBus, Action<CardInfoVisibilityChanged> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
