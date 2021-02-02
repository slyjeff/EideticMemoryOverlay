using ArkhamOverlay.TcpUtils.Requests;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Services;
using StreamDeckPlugin.Utils;
using System.Threading.Tasks;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Clear Cards", "arkhamoverlay.clearcards")]
    public class ClearCardsAction : StreamDeckAction {
        private readonly ISendSocketService _sendSocketService = ServiceLocator.GetService<ISendSocketService>();
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            _sendSocketService.SendRequest(new ClearAllCardsRequest());
            return Task.CompletedTask;
        }
    }
}