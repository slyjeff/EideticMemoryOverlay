using ArkhamOverlay.Common.Tcp.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ArkhamOverlay.Common.Services {
    public interface IEvent {
    }

    public interface ICrossAppEvent : IEvent {
    }

    public interface IEventBus {
        void Publish<T>(T eventToPublish) where T : IEvent;
        void Subscribe<T>(Action<T> callback) where T : IEvent;
        void Unsubscribe<T>(Action<T> callback) where T : IEvent;
    }

    public interface ICrossAppEventBus {
        event Action<EventBusRequest> SendMessage;
        void ReceiveMessage(EventBusRequest eventBusRequest);
    }

    public class EventBus : IEventBus, ICrossAppEventBus {
        private readonly object _subscriptionListLock = new object();
        private readonly IDictionary<Type, object> _subscriptionList = new Dictionary<Type, object>();

        public void Publish<T>(T eventToPublish) where T : IEvent {
            if (eventToPublish is ICrossAppEvent crossAppEventToPublish) {
                PublishCrossAppEvent(crossAppEventToPublish);
            }

            var type = typeof(T);
            if (!_subscriptionList.ContainsKey(type)) {
                return;
            }

            DoInvoke((Action<T>)_subscriptionList[type], eventToPublish);
        }

        private void PublishCrossAppEvent<T>(T eventToPublish) where T : ICrossAppEvent {
            var request = new EventBusRequest {
                EventClassName = eventToPublish.GetType().AssemblyQualifiedName,
                SerializedEventData = JsonConvert.SerializeObject(eventToPublish)
            };
            _sendMessage?.Invoke(request);
        }

        protected virtual void DoInvoke<T>(Action<T> action, T eventToPublish) where T : IEvent {
            action?.Invoke(eventToPublish);
        }

        protected virtual void Invoke(Type type, object action, ICrossAppEvent eventToPublish) {
            var doInvokeMethod = GetType().GetMethod(nameof(DoInvoke), BindingFlags.Instance | BindingFlags.NonPublic);
            var genericDoInvokeMethod = doInvokeMethod.MakeGenericMethod(type);

            var parameters = new object[] { action, eventToPublish };

            genericDoInvokeMethod.Invoke(this, parameters);
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

        private event Action<EventBusRequest> _sendMessage;
        event Action<EventBusRequest> ICrossAppEventBus.SendMessage {
            add { _sendMessage += value; }
            remove { _sendMessage -= value; }
        }

        void ICrossAppEventBus.ReceiveMessage(EventBusRequest eventBusRequest) {
            try {
                var type = Type.GetType(eventBusRequest.EventClassName);
                if (type == null) {
                    return;
                }

                if (!_subscriptionList.ContainsKey(type)) {
                    return;
                }

                var eventToPublish = (type.GetConstructors().FirstOrDefault() == null)
                    ? Activator.CreateInstance(type) as ICrossAppEvent
                    : Activator.CreateInstance(type, GetParameterListFromJsonData(type, eventBusRequest.SerializedEventData)) as ICrossAppEvent;

                Invoke(type, _subscriptionList[type], eventToPublish);
            } catch {
                ///todo: we need to inject the logger here, but that means also supporting this in the streamdeck plugin
            }
        }

        private object[] GetParameterListFromJsonData(Type type, string serializedData) {
            var o = JObject.Parse(serializedData);
            var parameters = new List<object>();

            var constructor = type.GetConstructors().First();
            var parameterList = constructor.GetParameters();
            foreach (var parameter in parameterList) {
                var parameterName = char.ToUpperInvariant(parameter.Name[0]) + parameter.Name.Substring(1);

                parameters.Add(o[parameterName].ToObject(parameter.ParameterType));
            }
            return parameters.ToArray();
        }
    }
}
