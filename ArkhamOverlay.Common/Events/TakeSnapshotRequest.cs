using ArkhamOverlay.Common.Services;
using System;

namespace ArkhamOverlay.Common.Events {
    public class TakeSnapshotRequest : ICrossAppEvent {
    }

    public static class TakeSnapshotRequestExtensions {
        public static void PublishTakeSnapshotRequest(this IEventBus eventBus) {
            eventBus.Publish(new TakeSnapshotRequest());
        }

        public static void SubscribeToTakeSnapshotRequest(this IEventBus eventBus, Action<TakeSnapshotRequest> action) {
            eventBus.Subscribe(action);
        }
        public static void UnsubscribeFromTakeSnapshotRequest(this IEventBus eventBus, Action<TakeSnapshotRequest> action) {
            eventBus.Unsubscribe(action);
        }
    }
}
