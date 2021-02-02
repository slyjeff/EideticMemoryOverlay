using System.Threading.Tasks;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Page Left", "arkhamoverlay.pageleft")]
    public class PageLeftAction : StreamDeckAction {
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            foreach (var cardButtonAction in CardButtonAction.ListOf) {
                if (!cardButtonAction.IsVisible) {
                    continue;
                }

                cardButtonAction.PreviousPage();
            }
            return Task.CompletedTask;
        }
    }
}