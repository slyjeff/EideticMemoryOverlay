using System.Threading.Tasks;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;

namespace ArkhamOverlaySdPlugin.Actions {
    [StreamDeckAction("Page Left", "arkhamoverlay.pageleft")]
    public class PageLeftAction : StreamDeckAction {
        protected async override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            foreach (var cardButtonAction in CardButtonAction.ListOf) {
                if (!cardButtonAction.IsVisible) {
                    continue;
                }

                if (cardButtonAction.Page > 0) {
                    cardButtonAction.Page--;
                }
                await cardButtonAction.GetButtonInfo();
            }
        }
    }
}