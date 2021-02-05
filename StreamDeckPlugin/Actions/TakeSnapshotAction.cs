using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Events;
using StreamDeckPlugin.Services;
using StreamDeckPlugin.Utils;
using System.Threading.Tasks;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Take Snapshot", "arkhamoverlay.takesnapshot")]
    public class TakeSnapshotAction : StreamDeckAction {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            _eventBus.TakeSnapshot();
            return Task.CompletedTask;
        }
    }
}