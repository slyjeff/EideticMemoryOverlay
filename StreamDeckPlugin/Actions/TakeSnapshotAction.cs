using ArkhamOverlay.TcpUtils.Requests;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Utils;
using System.Threading.Tasks;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Take Snapshot", "arkhamoverlay.takesnapshot")]
    public class TakeSnapshotAction : StreamDeckAction {
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            StreamDeckSendSocketService.SendRequest(new SnapshotRequest());
            return Task.CompletedTask;
        }
    }
}