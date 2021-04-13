using Emo.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public enum ChangePageDirection { Previous, Next }

    public class PageChangedEvent : IEvent {
        public PageChangedEvent(ChangePageDirection direction) {
            Direction = direction;
        }

        public ChangePageDirection Direction { get; }
    }

    public static class PageChangedEventExtensions {
        public static void PublishPageChangedEvent(this IEventBus eventBus, ChangePageDirection direction) {
            eventBus.Publish(new PageChangedEvent(direction));
        }

        public static void SubscribeToPageChangedEvent(this IEventBus eventBus, Action<PageChangedEvent> callback) {
            eventBus.Subscribe(callback);
        }

        public static void UnsubscribeFromPageChangedEvent(this IEventBus eventBus, Action<PageChangedEvent> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
