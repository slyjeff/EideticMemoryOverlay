using System;
using System.Windows;
using System.Windows.Threading;

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
            if (Application.Current.Dispatcher.CheckAccess()) {
                SnapshotRequested?.Invoke();
            } else {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    SnapshotRequested?.Invoke();
                }));
            }
        }
    }
}
