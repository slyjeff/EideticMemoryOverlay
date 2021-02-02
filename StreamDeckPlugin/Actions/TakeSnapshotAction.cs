using ArkhamOverlay.TcpUtils.Requests;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Services;
using StreamDeckPlugin.Utils;
using System.Threading.Tasks;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Take Snapshot", "arkhamoverlay.takesnapshot")]
    public class TakeSnapshotAction : StreamDeckAction {
        private readonly ISendSocketService _sendSocketService = ServiceLocator.GetService<ISendSocketService>();
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            _sendSocketService.SendRequest(new SnapshotRequest());
            return Task.CompletedTask;
        }
    }
}