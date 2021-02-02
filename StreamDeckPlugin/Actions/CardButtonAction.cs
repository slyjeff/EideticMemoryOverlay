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
using StreamDeckPlugin.Services;
using StreamDeckPlugin.Utils;

namespace StreamDeckPlugin.Actions {
    public class CardSettings {
        public string Deck { get; set; }
    }

    [StreamDeckAction("Card Button", "arkhamoverlay.cardbutton")]
    public class CardButtonAction : StreamDeckAction<CardSettings> {
        public static IList<CardButtonAction> ListOf = new List<CardButtonAction>();

        private readonly ISendSocketService _sendSocketService = ServiceLocator.GetService<ISendSocketService>();
        private readonly IDynamicActionService _dynamicActionService = ServiceLocator.GetService<IDynamicActionService>();
        private readonly IImageService _imageService = ServiceLocator.GetService<IImageService>();

        private Coordinates _coordinates = new Coordinates();
        private CardSettings _settings = new CardSettings();

        private string _deviceId;
        private Timer _keyPressTimer = new Timer(700);
        private string _lastSetTitle;
        private int _page;

        public bool IsVisible { get; private set; }


        public CardButtonAction() {
            ListOf.Add(this);
            _keyPressTimer.Enabled = false;
            _keyPressTimer.Elapsed += KeyHeldDown;
        }

        public DynamicActionMode Mode { get; private set; } 

        public Deck Deck {
            get {
                if (_settings == null) {
                    return Deck.Player1;
                }

                return _settings.Deck.AsDeck();
            }
        }

        public int Index {
            get {
                var rows = 4;
                var columns = 16;
                var device = StreamDeck.Info.Devices.FirstOrDefault(x => x.Id == _deviceId);
                if (device != null) {
                    rows = device.Size.Rows;
                    columns = device.Size.Columns;
                }

                var buttonsPerPage = rows * columns - 4; //3 because the return to parent, show hand, left, and right buttons take up four slots

                return (_page * buttonsPerPage) + (_coordinates.Row * columns + _coordinates.Column) - 1;
            }
        }

        protected override Task OnWillAppear(ActionEventArgs<AppearancePayload> args) {
            _coordinates = args.Payload.Coordinates;
            _deviceId = args.Device;
            _settings = args.Payload.GetSettings<CardSettings>();
            IsVisible = true;

            _dynamicActionService.DynamicActionChanged += DynamicActionChanged;

            SetMode(DynamicActionMode.Pool);
 
            return Task.CompletedTask;
        }

        protected override Task OnWillDisappear(ActionEventArgs<AppearancePayload> args) {
            _dynamicActionService.DynamicActionChanged -= DynamicActionChanged;
            IsVisible = false;
            return Task.CompletedTask;
        }

        protected override Task OnSendToPlugin(ActionEventArgs<JObject> args) {
            _settings.Deck = args.Payload["deck"].Value<string>();

            SetSettingsAsync(_settings);

            GetButtonInfo();

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
            var request = new ClickCardButtonRequest { Deck = _settings.Deck.AsDeck(), Index = Index, FromCardSet = Mode == DynamicActionMode.Set, Click = click };
            _sendSocketService.SendRequest<CardInfoResponse>(request);

            //setting the card name, just because we want the button to update to show the opration is finished (no longer have the "pressed in" look)
            SetTitleAsync(TextUtils.WrapTitle(_lastSetTitle));
        }

        private void GetButtonInfo() {
            var request = new GetCardInfoRequest { Deck = _settings.Deck.AsDeck(), Index = Index, FromCardSet = Mode == DynamicActionMode.Set};
            var cardInfo = _sendSocketService.SendRequest<CardInfoResponse>(request);

            _dynamicActionService.UpdateDynamicAction(Deck, Index, Mode, cardInfo);
        }

        private bool DynamicActionMatchesButton(IDynamicAction dynamicAction) {
            return (dynamicAction.Deck == Deck && dynamicAction.Index == Index && dynamicAction.Mode == Mode);
        }

        private void DynamicActionChanged(IDynamicAction dynamicAction) {
            //we don't know anything about ourselves yet, so we can't really respond to changes
            if (_deviceId == null) {
                return;
            }

            if (!DynamicActionMatchesButton(dynamicAction)) {
                return;
            }

            UpdateButtonDisplay(dynamicAction);
        }

        private void ImageLoaded(IDynamicAction dynamicAction) {
            if (!DynamicActionMatchesButton(dynamicAction)) {
                return;
            }

            _imageService.ImageLoaded -= ImageLoaded;
            SetImageAsync(_imageService.GetImage(dynamicAction));
        }

        public void SetMode(DynamicActionMode mode) {
            _page = 0;
            Mode = mode;

            UpdateButtonToNewDynamicAction();
        }

        public void NextPage() {
            _page++;

            UpdateButtonToNewDynamicAction();
        }

        public void PreviousPage() {
            if (_page == 0) {
                return;
            }
            _page--;

            UpdateButtonToNewDynamicAction();
        }

        private void UpdateButtonToNewDynamicAction() {
            var dynamicAction = _dynamicActionService.GetDynamicAction(Deck, Index, Mode);
            if (dynamicAction == null) {
                SetTitleAsync(string.Empty);
                SetImageAsync(string.Empty);

                GetButtonInfo();
                return;
            }
            UpdateButtonDisplay(dynamicAction);
        }

        private void UpdateButtonDisplay(IDynamicAction dynamicAction) {
            _lastSetTitle = dynamicAction.Text;
            SetTitleAsync(TextUtils.WrapTitle(_lastSetTitle));

            if (!dynamicAction.IsImageAvailable) {
                SetImageAsync(string.Empty);
            } else if (_imageService.HasImage(dynamicAction)) {
                SetImageAsync(_imageService.GetImage(dynamicAction));
            } else {
                _imageService.ImageLoaded += ImageLoaded;
            }
        }
    }
}