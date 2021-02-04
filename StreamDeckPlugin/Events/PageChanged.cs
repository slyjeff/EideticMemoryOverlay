using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public enum ChangePageDirection { Previous, Next }

    public class PageChanged : IEvent {
        public PageChanged(ChangePageDirection direction) {
            Direction = direction;
        }

        public ChangePageDirection Direction { get; }
    }

    public static class PageChangedExtensions {
        public static void ChangePage(this IEventBus eventBus, ChangePageDirection direction) {
            eventBus.Publish(new PageChanged(direction));
        }

        public static void Subscribe(this IEventBus eventBus, Action<PageChanged> action) {
            eventBus.Subscribe(action);
        }

        public static void Unsubscribe(this IEventBus eventBus, Action<PageChanged> action) {
            eventBus.Unsubscribe(action);
        }
    }
}
