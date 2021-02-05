using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class ClearAllCards : IEvent {
    }

    public static class ClearAllCardsExtensions {
        public static void ClearAllCards(this IEventBus eventBus) {
            eventBus.Publish(new ClearAllCards());
        }

        public static void OnClearAllCards(this IEventBus eventBus, Action action) {
            eventBus.Subscribe<ClearAllCards>(x => action?.Invoke());
        }
    }
}
