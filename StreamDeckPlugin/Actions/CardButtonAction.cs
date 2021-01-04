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
        private ICardInfo _currentCardInfo;

        public CardButtonAction() {
            ListOf.Add(this);
        }

        public int Page { get; set; }
        public bool IsVisible { get; private set; }

        public Deck Deck {
            get {
                if (_settings == null) {
                    return Deck.Player1;
                }

                return _settings.Deck.AsDeck();
            }
        }

        public int CardButtonIndex {
            get {
                var rows = 4;
                var columns = 16;
                var device = StreamDeck.Info.Devices.FirstOrDefault(x => x.Id == _deviceId);
                if (device != null) {
                    rows = device.Size.Rows;
                    columns = device.Size.Columns;
                }

                var buttonsPerPage = rows * columns - 3; //3 because the return to parent, left, and right buttons take up three slots
 
                return (Page * buttonsPerPage) + (_coordinates.Row * columns + _coordinates.Column) - 1;
            }
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

            //do this for the first cardbutton on the screen- we don't need a million requests going out
            if (CardButtonIndex == 0) {
                RegisterForButtonUpdates();
            }

            await GetButtonInfo();
        }

        private void RegisterForButtonUpdates() {
            //send this every time just to make sure the overlay is aware of us- otherwise we won't get updates
            var request = new RegisterForUpdatesRequest { Port = StreamDeckTcpInfo.Port };
            StreamDeckSendSocketService.SendRequest<OkResponse>(request);
        }

        protected override Task OnWillDisappear(ActionEventArgs<AppearancePayload> args) {
            IsVisible = false;
            return Task.CompletedTask;
        }

        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            var settings = args.Payload.GetSettings<CardButtonSettings>();
            try {
                var request = new ClickCardButtonRequest { Deck = settings.Deck.AsDeck(), Index = CardButtonIndex };
                var response = StreamDeckSendSocketService.SendRequest<CardInfoResponse>(request);

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
            try {
                var request = new GetCardInfoRequest { Deck = _settings.Deck.AsDeck(), Index = CardButtonIndex };
                var response = StreamDeckSendSocketService.SendRequest<CardInfoResponse>(request);

                await UpdateButtonInfo(response);
            } catch {
            }
        }

        public async Task UpdateButtonInfo(ICardInfo cardInfo) {
            if (string.IsNullOrEmpty(cardInfo.Name)) {
                await Clear();
            } else {
                if (_currentCardInfo != null) {
                    if (_currentCardInfo.CardButtonType == cardInfo.CardButtonType
                        && _currentCardInfo.Name == cardInfo.Name
                        && _currentCardInfo.IsVisible == cardInfo.IsVisible) {
                        return;
                    }
                }
                _currentCardInfo = cardInfo;

                await SetTitleAsync(TextUtils.WrapTitle(cardInfo.Name));
                await SetImageAsync(cardInfo.AsImage());
            }
        }
    }
}