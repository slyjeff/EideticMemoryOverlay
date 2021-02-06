using System;
using System.Collections.Generic;

namespace ArkhamOverlay.Common.Services {
    public interface IEvent {
    }

    public interface IEventBus {
        void Publish<T>(T eventToPublish) where T : IEvent;
        void Subscribe<T>(Action<T> callback) where T : IEvent;
        void Unsubscribe<T>(Action<T> callback) where T : IEvent;
    }

    public class EventBus : IEventBus {
        private readonly object _subscriptionListLock = new object();
        private readonly IDictionary<Type, object> _subscriptionList = new Dictionary<Type, object>();

        public void Publish<T>(T eventToPublish) where T : IEvent {
            var key = typeof(T);
            if (!_subscriptionList.ContainsKey(key)) {
                return;
            }


            DoInvoke((Action<T>)_subscriptionList[key], eventToPublish);
        }

        protected virtual void DoInvoke<T>(Action<T> action, T eventToPublish) where T : IEvent {
            action?.Invoke(eventToPublish);
        }

        public void Subscribe<T>(Action<T> callback) where T : IEvent {
            lock (_subscriptionListLock) {
                var key = typeof(T);
                if (!_subscriptionList.ContainsKey(key)) {
                    _subscriptionList.Add(key, callback);
                    return;
                }

                var subscriptions = (Action<T>)_subscriptionList[key];
                subscriptions += callback;
                _subscriptionList[key] = subscriptions;
            }
        }

        public void Unsubscribe<T>(Action<T> callback) where T : IEvent {
            lock (_subscriptionListLock) {
                var key = typeof(T);
                if (!_subscriptionList.ContainsKey(key)) {
                    _subscriptionList.Add(key, callback);
                    return;
                }

                var subscriptions = (Action<T>)_subscriptionList[key];
                subscriptions -= callback;
                _subscriptionList[key] = subscriptions;
            }
        }
    }
}
