using System.Threading.Tasks;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Toggle Set", "arkhamoverlay.toggleset")]
    public class ToggleSetAction : StreamDeckAction {
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            foreach (var dynamicAction in DynamicAction.ListOf) {
                if (!dynamicAction.IsVisible) {
                    continue;
                }

                dynamicAction.SetMode(dynamicAction.Mode == Services.DynamicActionMode.Pool ? Services.DynamicActionMode.Set : Services.DynamicActionMode.Pool);
            }
            
            return Task.CompletedTask;
        }
    }
}