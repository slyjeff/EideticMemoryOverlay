using ArkhamOverlay.Common.Services;
using System;
using System.Windows;
using System.Windows.Threading;

namespace ArkhamOverlay.Services {
    public class UiEventBus : EventBus {
        protected override void DoInvoke<T>(Action<T> action, T eventToPublish) {
            if (Application.Current.Dispatcher.CheckAccess()) {
                base.DoInvoke(action, eventToPublish);
            } else {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    base.DoInvoke(action, eventToPublish);
                }));
            }
       }
    }
}
