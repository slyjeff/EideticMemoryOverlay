using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using ArkhamOverlay.TcpUtils;
using Newtonsoft.Json.Linq;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Events;
using StreamDeckPlugin.Services;
using StreamDeckPlugin.Utils;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Dynamic Action", "arkhamoverlay.dynamicaction")]
    public class DynamicAction : StreamDeckAction<ActionWithDeckSettings>, IDynamicActionInfoContext {
        private readonly IDynamicActionInfoStore _dynamicActionInfoStore = ServiceLocator.GetService<IDynamicActionInfoStore>();
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        private readonly IImageService _imageService = ServiceLocator.GetService<IImageService>();

        private Coordinates _coordinates = new Coordinates();
        private ActionWithDeckSettings _settings = new ActionWithDeckSettings();

        private string _deviceId;
        private Timer _keyPressTimer = new Timer(700);
        private string _lastSetTitle;
        private int _page;

        public DynamicAction() {
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

                var buttonsPerPage = rows * columns - 4; //4 because the return to parent, show hand, left, and right buttons take up four slots

                return (_page * buttonsPerPage) + (_coordinates.Row * columns + _coordinates.Column) - 1;
            }
        }

        protected override Task OnWillAppear(ActionEventArgs<AppearancePayload> args) {
            _coordinates = args.Payload.Coordinates;
            _deviceId = args.Device;
            _settings = args.Payload.GetSettings<ActionWithDeckSettings>();
            
            _eventBus.SubscribeToDynamicActionInfoChangedEvent(DynamicActionChanged);
            _eventBus.SubscribeToPageChangedEvent(PageChanged);
            _eventBus.SubscribeToModeToggledEvent(ModeToggled);

            SetMode(DynamicActionMode.Pool);
 
            return Task.CompletedTask;
        }

        protected override Task OnWillDisappear(ActionEventArgs<AppearancePayload> args) {
            _eventBus.UnsubscribeFromModeToggledEvent(ModeToggled);
            _eventBus.UnsubscribeFromPageChangedEvent(PageChanged);
            _eventBus.UnsubscribeFromDynamicActionInfoChangedEvent(DynamicActionChanged);
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
            _settings = args.Payload.GetSettings<ActionWithDeckSettings>();
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

                SendClick(isLeftClick: false);
            }
        }

        protected override Task OnKeyUp(ActionEventArgs<KeyPayload> args) {
            lock (_keyUpLock) {
                if (!_keyIsDown) {
                    return Task.CompletedTask;
                }
                _keyIsDown = false;
                _keyPressTimer.Enabled = false;

                _settings = args.Payload.GetSettings<ActionWithDeckSettings>();
                SendClick(isLeftClick: true);

                return Task.CompletedTask;
            }
        }

        private void SendClick(bool isLeftClick) {
            _eventBus.PublishDynamicButtonClickRequest(Deck, Index, Mode, isLeftClick);

            //setting the card name, just because we want the button to update to show the opration is finished (no longer have the "pressed in" look)
            SetTitleAsync(TextUtils.WrapTitle(_lastSetTitle));
        }

        private void GetButtonInfo() {
            _eventBus.PublishGetButtonInfoRequest(Deck, Index, Mode);
        }

        private bool DynamicActionMatchesButton(IDynamicActionInfo dynamicActionInfo) {
            return (dynamicActionInfo.Deck == Deck && dynamicActionInfo.Index == Index && dynamicActionInfo.Mode == Mode);
        }

        private void DynamicActionChanged(DynamicActionInfoChangedEvent dynamicActionInfoChangedEvent) {
            //we don't know anything about ourselves yet, so we can't really respond to changes
            if (_deviceId == null) {
                return;
            }

            var dynamicActionInfo = dynamicActionInfoChangedEvent.DynamicActionInfo;

            if (!dynamicActionInfo.HasSameContext(this)) {
                return;
            }

            UpdateButtonDisplay(dynamicActionInfo);
        }

        private void PageChanged(PageChangedEvent pageChangedEvent) {
            if (pageChangedEvent.Direction == ChangePageDirection.Next) {
                _page++;
            } else {
                if (_page == 0) {
                    return;
                }
                _page--;
            }

            UpdateButtonToNewDynamicAction();
        }

        private void ModeToggled(ModeToggledEvent modeToggledEvent) {
            SetMode(Mode == DynamicActionMode.Pool ? DynamicActionMode.Set : DynamicActionMode.Pool);
        }

        public void SetMode(DynamicActionMode mode) {
            _page = 0;
            Mode = mode;

            UpdateButtonToNewDynamicAction();
        }

        private void UpdateButtonToNewDynamicAction() {
            var dynamicActionInfo = _dynamicActionInfoStore.GetDynamicActionInfo(Deck, Index, Mode);
            if (dynamicActionInfo == null) {
                SetTitleAsync(string.Empty);
                SetImageAsync(string.Empty);

                GetButtonInfo();
                return;
            }
            UpdateButtonDisplay(dynamicActionInfo);
        }

        private void UpdateButtonDisplay(IDynamicActionInfo dynamicActionInfo) {
            _lastSetTitle = dynamicActionInfo.Text;
            SetTitleAsync(TextUtils.WrapTitle(_lastSetTitle));

            if (_imageService.HasImage(dynamicActionInfo.ImageId)) {
                SetImageAsync(_imageService.GetImage(dynamicActionInfo));
            } else {
                ClearImage();
            }
        }

        private void ClearImage() {
            SetImageAsync(string.Empty);
        }
    }
}