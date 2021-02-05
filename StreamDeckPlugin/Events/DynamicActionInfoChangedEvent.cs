using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class DynamicActionInfoChangedEvent : IEvent {
        public DynamicActionInfoChangedEvent(IDynamicActionInfo dynamicActionInfo) {
            DynamicActionInfo = dynamicActionInfo;
        }

        public IDynamicActionInfo DynamicActionInfo { get; }
    }

    public static class DynamicActionInfoChangedEventExtensions {
        public static void DynamicActionInfoChanged(this IEventBus eventBus, IDynamicActionInfo dynamicActionInfo) {
            eventBus.Publish(new DynamicActionInfoChangedEvent(dynamicActionInfo));
        }

        public static void SubscribeToDynamicActionInfoChangedEvent(this IEventBus eventBus, Action<DynamicActionInfoChangedEvent> action) {
            eventBus.Subscribe(action);
        }

        public static void UnsubscribeFromDynamicActionInfoChangedEvent(this IEventBus eventBus, Action<DynamicActionInfoChangedEvent> callback) {
            eventBus.Unsubscribe(callback);
        }
    }
}
