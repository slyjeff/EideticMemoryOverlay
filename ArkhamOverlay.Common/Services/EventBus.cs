using ArkhamOverlay.Common.Tcp.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ArkhamOverlay.Common.Services {
    /// <summary>
    /// Implement this interface to pass as an event
    /// </summary>
    public interface IEvent {
    }

    /// <summary>
    /// Implement this interface to automatically send events to all apps
    /// </summary>
    /// <remarks>these events must initialze their public properties via the constructor, as that's how they will be initialized</remarks>
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

        /// <summary>
        /// Publish an event that will be sent to all subscribed event hanlders for that type of event, including in other apps
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventToPublish"></param>
        public void Publish<T>(T eventToPublish) where T : IEvent {
            if (eventToPublish is ICrossAppEvent crossAppEventToPublish) {
                PublishCrossAppEvent(crossAppEventToPublish);
            }

            var type = typeof(T);
            if (!_subscriptionList.ContainsKey(type)) {
                return;
            }

            DoInvokeCallbacks((Action<T>)_subscriptionList[type], eventToPublish);
        }

        /// <summary>
        /// Add a callback to a list to call when an even of this type is published, even if it was published from another app
        /// </summary>
        /// <typeparam name="T">The type of event</typeparam>
        /// <param name="callback">Callback to exectue when this event is raised</param>
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

        /// <summary>
        /// Remove the callback from the list to call when an event of this type is published
        /// </summary>
        /// <typeparam name="T">The type of event</typeparam>
        /// <param name="callback">Callback to exectue when this event is raised</param>
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

        /// <summary>
        /// An action raised when the event bus wants to send an event to another app
        /// </summary>
        public event Action<EventBusRequest> SendMessage;

        /// <summary>
        /// Receive a message via TCP- this is an event raised by another app
        /// </summary>
        /// <param name="eventBusRequest">The event body of the request</param>
        public void ReceiveMessage(EventBusRequest eventBusRequest) {
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

                InvokeCallbacks(type, _subscriptionList[type], eventToPublish);
            } catch {
                //todo: we need to inject the logger here, but that means also supporting this in the streamdeck plugin
            }
        }

        /// <summary>
        /// Overridable method that calls a list of callbacks with an event
        /// </summary>
        /// <typeparam name="T">The type of event to the callback expects</typeparam>
        /// <param name="action">Callbacks to handle the event</param>
        /// <param name="eventToPublish">Event that has been published, containing all necessary data to respond to it</param>
        protected virtual void DoInvokeCallbacks<T>(Action<T> action, T eventToPublish) where T : IEvent {
            action?.Invoke(eventToPublish);
        }

        /// <summary>
        /// A non-generic version of the DoInvokeCallback that calls the generic version of the DoInvokeCallback
        /// </summary>
        /// <param name="type">The type of event to the callback expects</param>
        /// <param name="action">Callbacks to handle the event</param>
        /// <param name="eventToPublish">Event that has been published, containing all necessary data to respond to it</param>
        protected virtual void InvokeCallbacks(Type type, object action, ICrossAppEvent eventToPublish) {
            var doInvokeMethod = GetType().GetMethod(nameof(DoInvokeCallbacks), BindingFlags.Instance | BindingFlags.NonPublic);
            var genericDoInvokeMethod = doInvokeMethod.MakeGenericMethod(type);

            var parameters = new object[] { action, eventToPublish };

            genericDoInvokeMethod.Invoke(this, parameters);
        }

        /// <summary>
        /// Invokes a SendMessag event that contains the event serialzed into a message that can be sent to other apps automatically
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventToPublish"></param>
        private void PublishCrossAppEvent<T>(T eventToPublish) where T : ICrossAppEvent {
            var request = new EventBusRequest {
                EventClassName = eventToPublish.GetType().AssemblyQualifiedName,
                SerializedEventData = JsonConvert.SerializeObject(eventToPublish)
            };
            SendMessage?.Invoke(request);
        }

        /// <summary>
        /// Look at the constructor of a type and then construct a list of parameters to pass to it from JSON data
        /// </summary>
        /// <param name="type">Look at the constructor for this type</param>
        /// <param name="serializedData">JSON that contains the values to pass to the constructor</param>
        /// <exception cref="JsonReaderException">Thrown if the json is not readable</exception>
        /// <returns>A list of parameters necessary to populate the constructor of the passed in type</returns>
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
