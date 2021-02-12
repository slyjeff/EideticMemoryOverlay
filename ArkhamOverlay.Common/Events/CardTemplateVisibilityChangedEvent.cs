using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Events {
    public class CardTemplateVisibilityChangedEvent : ICrossAppEvent {
        public CardTemplateVisibilityChangedEvent(string name, bool isVisible) {
            Name = name;
            IsVisible = isVisible;
        }

        public string Name;
        public bool IsVisible;
    }

    public static class CardTemplateVisibilityChangedEventExtensions {
        public static void PublishCardTemplateVisibilityChangedEvent(this IEventBus eventBus, string name, bool isVisible) {
            eventBus.Publish(new CardTemplateVisibilityChangedEvent(name, isVisible));
        }

        public static void SubscribeToCardTemplateVisibilityChangedEvent(this IEventBus eventBus, Action<CardTemplateVisibilityChangedEvent> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
