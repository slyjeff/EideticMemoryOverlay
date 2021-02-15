using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Data;
using System;

namespace ArkhamOverlay.Events {
    public class CardZoneVisibilityToggled : IEvent {
        public CardZoneVisibilityToggled(CardZone cardZone, bool isVisible) {
            CardZone = cardZone;
            IsVisible = isVisible;
        }

        public CardZone CardZone { get; }
        public bool IsVisible { get; }
    }

    public static class CardZoneVisibilityToggledExtensions {
        public static void PublishCardZoneVisibilityToggled(this IEventBus eventBus, CardZone cardZone, bool isVisible) {
            eventBus.Publish(new CardZoneVisibilityToggled(cardZone, isVisible));
        }

        public static void SubscribeToCardZoneVisibilityToggled(this IEventBus eventBus, Action<CardZoneVisibilityToggled> callback) {
            eventBus.Subscribe(callback);
        }

        public static void UnsubscribeFromCardZoneVisibilityToggled(this IEventBus eventBus, Action<CardZoneVisibilityToggled> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
