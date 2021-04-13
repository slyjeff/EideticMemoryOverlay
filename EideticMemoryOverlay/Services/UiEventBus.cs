using Emo.Common.Services;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Emo.Services {
    public class UiEventBus : EventBus {
        protected override void DoInvokeCallbacks<T>(Action<T> action, T eventToPublish) {
            //todo: think about if we need to be smarter about this and provide opt in/ opt out for forcing events to execute on the main UI thread
            if (Application.Current.Dispatcher.CheckAccess()) {
                base.DoInvokeCallbacks(action, eventToPublish);
            } else {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    base.DoInvokeCallbacks(action, eventToPublish);
                }));
            }
       }
    }
}
