using ArkhamOverlay.Common.Services;
using StreamDeckPlugin.Services;
using System;

namespace StreamDeckPlugin.Events {
    public class EstablishConnectionToUiRequest : IEvent {
    }

    public static class EstablishConnectionToUiRequestExtensions {
        public static void PublishEstablishConnectionToUiRequest(this IEventBus eventBus) {
            eventBus.Publish(new EstablishConnectionToUiRequest());
        }

        public static void SubscribeToEstablishConnectionToUiRequest(this IEventBus eventBus, Action<EstablishConnectionToUiRequest> callback) {
            eventBus.Subscribe(callback);
        }
    }
}
