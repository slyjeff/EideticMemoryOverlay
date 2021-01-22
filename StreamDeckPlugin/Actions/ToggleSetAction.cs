using System.Threading.Tasks;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Toggle Set", "arkhamoverlay.toggleset")]
    public class ToggleSetAction : StreamDeckAction {
        protected async override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            foreach (var cardButtonAction in CardButtonAction.ListOf) {
                if (!cardButtonAction.IsVisible) {
                    continue;
                }

                cardButtonAction.Page = 0;
                cardButtonAction.ShowCardSet = !cardButtonAction.ShowCardSet;

                await cardButtonAction.GetButtonInfo();
            }
        }
    }
}