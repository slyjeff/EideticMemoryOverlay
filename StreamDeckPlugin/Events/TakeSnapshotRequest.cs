using ArkhamOverlay.Common.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class TakeSnapshotRequest : IEvent {
    }

    public static class TakeSnapshotRequestExtensions {
        public static void PublishTakeSnapshotRequest(this IEventBus eventBus) {
            eventBus.Publish(new TakeSnapshotRequest());
        }

        public static void SubscribeToTakeSnapshotRequest(this IEventBus eventBus, Action<TakeSnapshotRequest> action) {
            eventBus.Subscribe(action);
        }
    }
}
