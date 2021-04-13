using System.Threading.Tasks;
using Emo.Common.Services;
using Emo.Common.Utils;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Events;

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