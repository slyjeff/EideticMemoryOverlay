using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using Newtonsoft.Json.Linq;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Utils;

namespace ArkhamOverlaySdPlugin.Actions {
    public class CardSettings {
        public string Deck { get; set; }
    }

    [StreamDeckAction("Card Button", "arkhamoverlay.cardbutton")]
    public class CardButtonAction : StreamDeckAction<CardSettings> {
        public static IList<CardButtonAction> ListOf = new List<CardButtonAction>();

        private Coordinates _coordinates = new Coordinates();
        private CardSettings _settings = new CardSettings();
        private string _deviceId;
        private ICardInfo _currentCardInfo;
        private Timer _keyPressTimer = new Timer(1000);

        public CardButtonAction() {
            ListOf.Add(this);
            _keyPressTimer.Enabled = false;
            _keyPressTimer.Elapsed += OneSecondAfterKeyDown;
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
            _settings = args.Payload.GetSettings<CardSettings>();
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


        private bool _keyIsDown = false;
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            _settings = args.Payload.GetSettings<CardSettings>();
            _keyIsDown = true;

            _keyPressTimer.Enabled = true;

            return Task.CompletedTask;
        }

        private void OneSecondAfterKeyDown(object sender, ElapsedEventArgs e) {
            if (!_keyIsDown) {
                return;
            }
            _keyIsDown = false;
            _keyPressTimer.Enabled = false;

            SendClick(ButtonClick.Right);
        }


        protected override Task OnKeyUp(ActionEventArgs<KeyPayload> args) {
            if (!_keyIsDown) {
                return Task.CompletedTask;
            }
            _keyIsDown = false;

            _settings = args.Payload.GetSettings<CardSettings>();
            SendClick(ButtonClick.Left);
            return Task.CompletedTask;
        }

        private void SendClick(ButtonClick click) {
            try {
                var request = new ClickCardButtonRequest { Deck = _settings.Deck.AsDeck(), Index = CardButtonIndex, Click = click };
                var response = StreamDeckSendSocketService.SendRequest<CardInfoResponse>(request);

                SetImageAsync(response.AsImage());
                SetTitleAsync(TextUtils.WrapTitle(response.Name));
            } catch {
                SetTitleAsync("");
            }
        }

        public async Task Clear() {
            if (_currentCardInfo == null) {
                return;
            }

            _currentCardInfo.CardButtonType = CardButtonType.Unknown;
            _currentCardInfo.Name = string.Empty;
            _currentCardInfo.IsVisible = false;
            
            await SetTitleAsync(string.Empty);
            await SetImageAsync(ImageUtils.BlankImage());
        }

        public async Task GetButtonInfo() {
            try {
                var request = new GetCardInfoRequest { Deck = _settings.Deck.AsDeck(), Index = CardButtonIndex};
                var cardInfo = StreamDeckSendSocketService.SendRequest<CardInfoResponse>(request);

                await UpdateButtonInfo(cardInfo);
            } catch {
            }
        }

        private Task GetButtonImage() {
            var request = new ButtonImageRequest { Deck = _settings.Deck.AsDeck(), Index = CardButtonIndex };
            var response = StreamDeckSendSocketService.SendRequest<ButtonImageResponse>(request);
            ImageUtils.ImageCache[response.Name] = response.Bytes;
            return Task.CompletedTask;
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

                if (cardInfo.ImageAvailable && !ImageUtils.ImageCache.ContainsKey(cardInfo.Name)) {
                    await GetButtonImage();
                }

                await SetTitleAsync(TextUtils.WrapTitle(cardInfo.Name));
                await SetImageAsync(cardInfo.AsImage());
            }
        }
    }
}