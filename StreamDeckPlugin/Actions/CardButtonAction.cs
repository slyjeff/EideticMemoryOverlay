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

namespace StreamDeckPlugin.Actions {
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
        private Timer _keyPressTimer = new Timer(700);

        public CardButtonAction() {
            ListOf.Add(this);
            _keyPressTimer.Enabled = false;
            _keyPressTimer.Elapsed += KeyHeldDown;
        }
        public int Page { get; set; }
        public bool ShowCardSet { get; set; }
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

                var buttonsPerPage = rows * columns - 4; //3 because the return to parent, show hand, left, and right buttons take up four slots
 
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
            ShowCardSet = false;
            IsVisible = true;

            await GetButtonInfo();
        }

        protected override Task OnWillDisappear(ActionEventArgs<AppearancePayload> args) {
            IsVisible = false;
            return Task.CompletedTask;
        }

        private object _keyUpLock = new object();
        private bool _keyIsDown = false;
        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            _settings = args.Payload.GetSettings<CardSettings>();
            _keyIsDown = true;

            _keyPressTimer.Enabled = true;

            return Task.CompletedTask;
        }

        private void KeyHeldDown(object sender, ElapsedEventArgs e) {
            lock (_keyUpLock) {
                if (!_keyIsDown) {
                    return;
                }
                _keyIsDown = false;
                _keyPressTimer.Enabled = false;

                SendClick(ButtonClick.Right);
            }
        }


        protected override Task OnKeyUp(ActionEventArgs<KeyPayload> args) {
            lock (_keyUpLock) {
                if (!_keyIsDown) {
                    return Task.CompletedTask;
                }
                _keyIsDown = false;
                _keyPressTimer.Enabled = false;

                _settings = args.Payload.GetSettings<CardSettings>();
                SendClick(ButtonClick.Left);
                return Task.CompletedTask;
            }
        }

        private void SendClick(ButtonClick click) {
            var request = new ClickCardButtonRequest { Deck = _settings.Deck.AsDeck(), Index = CardButtonIndex, FromCardSet = ShowCardSet, Click = click };
            StreamDeckSendSocketService.SendRequest<CardInfoResponse>(request);
            
            //setting the card name, just because we want the button to update to show the opration is finished (no longer have the "pressed in" look
            if (_currentCardInfo != null) {
                SetTitleAsync(TextUtils.WrapTitle(_currentCardInfo.Name));
            }
        }

        public async Task Clear() {
            if (_currentCardInfo == null) {
                return;
            }

            _currentCardInfo.CardButtonType = CardButtonType.Unknown;
            _currentCardInfo.Name = string.Empty;
            _currentCardInfo.IsToggled = false;
            
            await SetTitleAsync(string.Empty);
            await SetImageAsync(ImageUtils.BlankImage());
        }

        public async Task GetButtonInfo() {
            try {
                var request = new GetCardInfoRequest { Deck = _settings.Deck.AsDeck(), Index = CardButtonIndex, FromCardSet = ShowCardSet};
                var cardInfo = StreamDeckSendSocketService.SendRequest<CardInfoResponse>(request);

                await UpdateButtonInfo(cardInfo);
            } catch {
            }
        }

        private Task GetButtonImage() {
            var request = new ButtonImageRequest { Deck = _settings.Deck.AsDeck(), Index = CardButtonIndex, FromCardSet = ShowCardSet };
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
                        && _currentCardInfo.IsToggled == cardInfo.IsToggled) {
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