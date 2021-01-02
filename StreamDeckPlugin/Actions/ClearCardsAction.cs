using System.Threading.Tasks;
using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;

namespace ArkhamOverlaySdPlugin.Actions {
    [StreamDeckAction("Clear Cards", "arkhamoverlay.clearcards")]
    public class ClearCardsAction : StreamDeckAction {
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            var response = SendSocketService.SendRequest(new ClearAllCardsRequest());
            return Task.CompletedTask;
        }
    }
}