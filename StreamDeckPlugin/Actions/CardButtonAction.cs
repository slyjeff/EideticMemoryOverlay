using System.Collections.Generic;
using System.Threading.Tasks;
using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using Newtonsoft.Json.Linq;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Utils;

namespace ArkhamOverlaySdPlugin.Actions {
    public class CardButtonSettings {
        public string Deck { get; set; }
    }

    [StreamDeckAction("Card Button", "arkhamoverlay.cardbutton")]
    public class CardButtonAction : StreamDeckAction<CardButtonSettings> {
        public static IList<CardButtonAction> ListOf = new List<CardButtonAction>();

        private Coordinates _coordinates = new Coordinates();
        private CardButtonSettings _settings = new CardButtonSettings();
        public int Page { get; set; }

        public CardButtonAction() {
            ListOf.Add(this);
        }

        protected async override Task OnSendToPlugin(ActionEventArgs<JObject> args) {
            _settings.Deck = args.Payload["deck"].Value<string>();
            
            await SetSettingsAsync(_settings);

            await GetButtonInfo();
        }

        protected async override Task OnWillAppear(ActionEventArgs<AppearancePayload> args) {
            _coordinates = args.Payload.Coordinates;
            _settings = args.Payload.GetSettings<CardButtonSettings>();
            Page = 0;
            await GetButtonInfo();
        }

        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            var settings = args.Payload.GetSettings<CardButtonSettings>();
            var cardIndex = GetCardButtonIndex(args.Payload.Coordinates);
            try {
                var request = new ClickCardButtonRequest { Deck = settings.Deck.AsDeck(), Index = cardIndex };
                var response = SendSocketService.SendRequest<CardInfoReponse>(request);

                SetImageAsync(response.CardButtonType.AsImage());
                return SetTitleAsync(TextUtils.WrapTitle(response.Name));
            } catch {
                return SetTitleAsync("");
            }
        }

        public async Task GetButtonInfo() {
            var cardIndex = GetCardButtonIndex(_coordinates);
            try {
                var request = new GetCardInfoRequest { Deck = _settings.Deck.AsDeck(), Index = cardIndex };
                var response = SendSocketService.SendRequest<CardInfoReponse>(request);

                await SetTitleAsync(TextUtils.WrapTitle(response.Name));
                await SetImageAsync(response.CardButtonType.AsImage());
            } catch {
            }
        }

        private int GetCardButtonIndex(Coordinates coordinates) {
            //assume 8 cards per row, as this app only makes since on the large streamdeck
            //subtract one for the "Return" location, since this will be in a folder
            //return (coordinates.Row * 8 + coordinates.Column - 1);

            //while developing on my phone, use 5
            return (Page * 12) + (coordinates.Row * 5 + coordinates.Column - 1);
        }

    }
}