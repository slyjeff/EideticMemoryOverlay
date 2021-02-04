using System.Threading.Tasks;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Page Right", "arkhamoverlay.pageright")]
    public class PageRightAction : StreamDeckAction {
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            foreach (var dynamicAction in DynamicAction.ListOf) {
                if (!dynamicAction.IsVisible) {
                    continue;
                }

                dynamicAction.NextPage();
            }
            return Task.CompletedTask;
        }
    }
}