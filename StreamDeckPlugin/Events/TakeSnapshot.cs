using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class TakeSnapshot : IEvent {
    }

    public static class TakeSnapshotExtensions {
        public static void TakeSnapshot(this IEventBus eventBus) {
            eventBus.Publish(new TakeSnapshot());
        }

        public static void OnTakeSnapshot(this IEventBus eventBus, Action action) {
            eventBus.Subscribe<TakeSnapshot>(x => action?.Invoke());
        }
    }
}
