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
        protected override Task OnSendToPlugin(ActionEventArgs<JObject> args) {
            var deck = args.Payload["deck"].Value<string>();
            var settings = new CardButtonSettings {
                Deck = deck
            };

            SetSettingsAsync(settings);

            var coordinates = new Coordinates {
                Column = args.Payload["coordinates"]["column"].Value<int>(),
                Row = args.Payload["coordinates"]["row"].Value<int>()
            };
            GetButtonInfo(settings, coordinates);

            return Task.CompletedTask;
        }

        protected override Task OnWillAppear(ActionEventArgs<AppearancePayload> args) {
            var settings = args.Payload.GetSettings<CardButtonSettings>();
            GetButtonInfo(settings, args.Payload.Coordinates);
            
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

        private void GetButtonInfo(CardButtonSettings settings, Coordinates coordinates) {
            var cardIndex = GetCardButtonIndex(coordinates);
            try {
                var request = new GetCardInfoRequest { Deck = settings.Deck.AsDeck(), Index = cardIndex };
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
            //assume 8 cards per row, as this app only makes since on the large streamdeck
            //subtract one for the "Return" location, since this will be in a folder
            //return (coordinates.Row * 8 + coordinates.Column - 1);

            //while developoing on my phone, use 5
            return (coordinates.Row * 5 + coordinates.Column - 1);

        }

    }
}