using System.Threading.Tasks;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Events;
using StreamDeckPlugin.Services;
using StreamDeckPlugin.Utils;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Page Right", "arkhamoverlay.pageright")]
    public class PageRightAction : StreamDeckAction {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();

        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            _eventBus.PublishPageChangedEvent(ChangePageDirection.Next);
            return Task.CompletedTask;
        }
    }
}