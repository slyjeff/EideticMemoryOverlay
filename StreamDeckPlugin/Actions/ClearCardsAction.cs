using ArkhamOverlay.Common.Events;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using System.Threading.Tasks;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Clear Cards", "arkhamoverlay.clearcards")]
    public class ClearCardsAction : StreamDeckAction {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            _eventBus.PublishClearAllCardsRequest();
            return Task.CompletedTask;
        }
    }
}