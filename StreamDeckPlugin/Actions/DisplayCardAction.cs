namespace ArkhamOverlaySdPlugin.Actions {
    using System.Threading.Tasks;
    using SharpDeck;
    using SharpDeck.Events.Received;
    using SharpDeck.Manifest;

    [StreamDeckAction("Display Card", "arkhamoverlay.displaycard")]
    public class DisplayCardAction : StreamDeckAction<Card> {
        protected override Task OnWillAppear(ActionEventArgs<AppearancePayload> args) {
            var card = args.Payload.GetSettings<Card>();
            return SetTitleAsync(card.Name);
        }

        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            // increment the count
            var card = args.Payload.GetSettings<Card>();
            card.Name = "I Had\r\n A Plan";

            // save the settings, and set the title
            SetSettingsAsync(card);
            return SetTitleAsync(card.Name);
        }
    }
}