using System.Collections.Generic;
using System.Linq;
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
        private string _deviceId;

        public int Page { get; set; }
        public bool IsVisible { get; private set; }

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
            _deviceId = args.Device;
            _settings = args.Payload.GetSettings<CardButtonSettings>();
            Page = 0;
            IsVisible = true;
            await GetButtonInfo();
        }

        protected override Task OnWillDisappear(ActionEventArgs<AppearancePayload> args) {
            IsVisible = false;
            return Task.CompletedTask;
        }

        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            var settings = args.Payload.GetSettings<CardButtonSettings>();
            var cardIndex = GetCardButtonIndex(args.Payload.Coordinates);
            try {
                var request = new ClickCardButtonRequest { Deck = settings.Deck.AsDeck(), Index = cardIndex };
                var response = SendSocketService.SendRequest<CardInfoResponse>(request);

                SetImageAsync(response.AsImage());
                return SetTitleAsync(TextUtils.WrapTitle(response.Name));
            } catch {
                return SetTitleAsync("");
            }
        }

        public async Task Clear() {
            await SetTitleAsync(string.Empty);
            await SetImageAsync(ImageUtils.BlankImage());
        }

        public async Task GetButtonInfo() {
            var cardIndex = GetCardButtonIndex(_coordinates);
            try {
                var request = new GetCardInfoRequest { Deck = _settings.Deck.AsDeck(), Index = cardIndex };
                var response = SendSocketService.SendRequest<CardInfoResponse>(request);

                if (string.IsNullOrEmpty(response.Name)) {
                    await Clear();
                } else {
                    await SetTitleAsync(TextUtils.WrapTitle(response.Name));
                    await SetImageAsync(response.AsImage());
                }
            } catch {
            }
        }

        private int GetCardButtonIndex(Coordinates coordinates) {
            var rows = 4;
            var columns = 16;
            var device = StreamDeck.Info.Devices.FirstOrDefault(x => x.Id == _deviceId);
            if (device != null) {
                rows = device.Size.Rows;
                columns = device.Size.Columns;
            }

            var buttonsPerPage = rows * columns - 3; //3 because the return to parent, left, and right buttons take up three slots

            //while developing on my phone, use 5
            return (Page * buttonsPerPage) + (coordinates.Row * columns + coordinates.Column) - 1;
        }

    }
}