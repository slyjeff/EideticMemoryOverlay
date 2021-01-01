using System.Threading.Tasks;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;

namespace ArkhamOverlaySdPlugin.Actions {
    [StreamDeckAction("Page Right", "arkhamoverlay.pageright")]
    public class PageRightAction : StreamDeckAction {
        protected async override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            foreach (var cardButtonAction in CardButtonAction.ListOf) {
                if (!cardButtonAction.IsVisible) {
                    continue;
                }

                cardButtonAction.Page++;
                await cardButtonAction.GetButtonInfo();
            }
        }
    }
}