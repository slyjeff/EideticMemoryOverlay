using ArkhamOverlay.Common.Events;
using ArkhamOverlay.Common.Services;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Utils;
using System.Threading.Tasks;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Take Snapshot", "arkhamoverlay.takesnapshot")]
    public class TakeSnapshotAction : StreamDeckAction {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            _eventBus.PublishTakeSnapshotRequest();
            return Task.CompletedTask;
        }
    }
}