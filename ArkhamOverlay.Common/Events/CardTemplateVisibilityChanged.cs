using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Events {
    public class CardTemplateVisibilityChanged : ICrossAppEvent {
        public CardTemplateVisibilityChanged(string name, bool isVisible) {
            Name = name;
            IsVisible = isVisible;
        }

        public string Name;
        public bool IsVisible;
    }

    public static class CardTemplateVisibilityChangedExtensions {
        public static void PublishCardTemplateVisibilityChanged(this IEventBus eventBus, string name, bool isVisible) {
            eventBus.Publish(new CardTemplateVisibilityChanged(name, isVisible));
        }

        public static void SubscribeToCardTemplateVisibilityChanged(this IEventBus eventBus, Action<CardTemplateVisibilityChanged> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
