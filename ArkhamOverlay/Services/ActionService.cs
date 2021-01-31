using System;

namespace ArkhamOverlay.Services {
    public interface IActionRequestService {
        void TakeSnapshot();
    }

    public interface IActionNotificationService {
        event Action SnapshotRequested;
    }

    class ActionService : IActionRequestService, IActionNotificationService {
        public event Action SnapshotRequested;

        public void TakeSnapshot() {
            SnapshotRequested?.Invoke();
        }
    }
}
