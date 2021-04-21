using Emo.Common.Events;
using Emo.Common.Services;
using Emo.Common.Utils;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using System.Threading.Tasks;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Take Snapshot", "emo.takesnapshot")]
    public class TakeSnapshotAction : StreamDeckAction {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            _eventBus.PublishTakeSnapshotRequest();
            return Task.CompletedTask;
        }
    }
}