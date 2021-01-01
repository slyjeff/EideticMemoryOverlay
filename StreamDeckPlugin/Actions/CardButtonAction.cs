using System;
using System.Drawing;
using System.Net;
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

                if (string.IsNullOrEmpty(response.ImageSource)) {
                    SetImageAsync(response.CardButtonType.AsImage());
                    SetTitleAsync(TextUtils.WrapTitle(response.Name));
                } else {
                    SetTitleAsync(TextUtils.WrapTitle(response.Name));
                    var imageRequest = WebRequest.Create("https://arkhamdb.com/" + response.ImageSource);
                    var imageResponse = imageRequest.GetResponse();                    
                    var responseStream = imageResponse.GetResponseStream();
                    var bitmap = new Bitmap(responseStream);

                    var cropRect = new Rectangle(new Point(40, 40), new System.Drawing.Size(220, 200));
                    var croppedBitmap = new Bitmap(cropRect.Width, cropRect.Height);

                    using (Graphics g = Graphics.FromImage(croppedBitmap)) {
                        g.DrawImage(bitmap, new Rectangle(0, 0, croppedBitmap.Width, croppedBitmap.Height), cropRect, GraphicsUnit.Pixel);
                    }

                    var converter = new ImageConverter();
                    var converted = (byte[])converter.ConvertTo(croppedBitmap, typeof(byte[]));
                    var imageString = Convert.ToBase64String(converted);

                    SetImageAsync("data:image/png;base64," + imageString);
                    //var bitmap = new BitmapImage(new Uri("https://arkhamdb.com/" + response.ImageSource, UriKind.Absolute));
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