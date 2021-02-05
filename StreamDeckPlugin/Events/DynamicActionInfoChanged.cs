using StreamDeckPlugin.Services;
using StreamDeckPlugin.Utils;
using System;

namespace StreamDeckPlugin.Events {
    public class DynamicActionInfoChanged : IEvent {
        public DynamicActionInfoChanged(IDynamicActionInfo dynamicActionInfo) {
            DynamicActionInfo = dynamicActionInfo;
        }

        public IDynamicActionInfo DynamicActionInfo { get; }
    }

    public static class DynamicActionInfoChangedExtensions {
        public static void DynamicActionInfoChanged(this IEventBus eventBus, IDynamicActionInfo dynamicActionInfo) {
            eventBus.Publish(new DynamicActionInfoChanged(dynamicActionInfo));
        }

        public static void OnDynamicActionInfoChanged(this IEventBus eventBus, Action<IDynamicActionInfo> action) {
            eventBus.Subscribe<DynamicActionInfoChanged>(x => action?.Invoke(x.DynamicActionInfo));
        }

        public static void Subscribe(this IEventBus eventBus, Action<DynamicActionInfoChanged> action) {
            eventBus.Subscribe(action);
        }

        public static void Unsubscribe(this IEventBus eventBus, Action<DynamicActionInfoChanged> action) {
            eventBus.Unsubscribe(action);
        }
    }
}
