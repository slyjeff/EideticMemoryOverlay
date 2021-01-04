using ArkhamOverlay.TcpUtils.Requests;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Utils;
using System.Threading.Tasks;

namespace ArkhamOverlaySdPlugin.Actions {
    [StreamDeckAction("Clear Cards", "arkhamoverlay.clearcards")]
    public class ClearCardsAction : StreamDeckAction {
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            StreamDeckSendSocketService.SendRequest(new ClearAllCardsRequest());
            return Task.CompletedTask;
        }
    }
}