using System.Threading.Tasks;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Page Right", "arkhamoverlay.pageright")]
    public class PageRightAction : StreamDeckAction {
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            foreach (var cardButtonAction in CardButtonAction.ListOf) {
                if (!cardButtonAction.IsVisible) {
                    continue;
                }

                cardButtonAction.NextPage();
            }
            return Task.CompletedTask;
        }
    }
}