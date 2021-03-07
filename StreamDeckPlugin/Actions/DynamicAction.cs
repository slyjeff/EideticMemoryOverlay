using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Events;
using Newtonsoft.Json.Linq;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using StreamDeckPlugin.Events;
using StreamDeckPlugin.Services;
using StreamDeckPlugin.Utils;

namespace StreamDeckPlugin.Actions {
    [StreamDeckAction("Dynamic Action", "arkhamoverlay.dynamicaction")]
    public class DynamicAction : StreamDeckAction<ActionWithDeckSettings> {
        private readonly IDynamicActionInfoStore _dynamicActionInfoStore = ServiceLocator.GetService<IDynamicActionInfoStore>();
        private readonly IDynamicActionManager _dynamicActionManager = ServiceLocator.GetService<IDynamicActionManager>();
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        private readonly IImageService _imageService = ServiceLocator.GetService<IImageService>();

        private IDynamicActionInfo _dynamicActionInfo;
        private Coordinates _coordinates = new Coordinates();
        private ActionWithDeckSettings _settings = new ActionWithDeckSettings();

        private string _deviceId;
        private readonly Timer _keyPressTimer = new Timer(700);
        private string _lastSetTitle;
        private int _page;

        private bool _initializing = false;
        private DynamicActionOption _dynamicActionOption;

        public DynamicAction() {
            _keyPressTimer.Enabled = false;
            _keyPressTimer.Elapsed += KeyHeldDown;
            ButtonMode = ButtonMode.Pool;
        }

        public ButtonMode ButtonMode { get; private set; } 

        public CardGroupId CardGroupId {
            get {
                if (_settings == null) {
                    return CardGroupId.Player1;
                }

                return _settings.Deck.AsCardGroupId();
            }
        }

        public int Page { get { return _page; } }
        public int PhysicalRow { get { return _coordinates.Row; } }
        public int PhysicalColumn { get { return _coordinates.Column; } }

        /// <summary>
        /// The index of the Dynamic Action determined by its physical location
        /// </summary>
        public int PhysicalIndex {
            get {
                var rows = 4;
                var columns = 16;
                var device = StreamDeck.Info.Devices.FirstOrDefault(x => x.Id == _deviceId);
                if (device != null) {
                    rows = device.Size.Rows;
                    columns = device.Size.Columns;
                }
                return _coordinates.Column + _coordinates.Row * columns;
            }
        }

        protected override Task OnWillAppear(ActionEventArgs<AppearancePayload> args) {
            _coordinates = args.Payload.Coordinates;
            _deviceId = args.Device;
            _settings = args.Payload.GetSettings<ActionWithDeckSettings>();

            _eventBus.SubscribeToPageChangedEvent(PageChanged);
            _eventBus.SubscribeToModeToggledEvent(ModeToggled);

            _initializing = true;
            var delayUpdateTimer = new Timer(50);
            delayUpdateTimer.Elapsed += (s, e) => {
                delayUpdateTimer.Enabled = false;
                _initializing = true;
                UpdateButtonToNewDynamicAction();
            };
            delayUpdateTimer.Enabled = true;

            _dynamicActionManager.RegisterAction(this);

            return Task.CompletedTask;
        }

        protected override Task OnWillDisappear(ActionEventArgs<AppearancePayload> args) {
            _eventBus.UnsubscribeFromModeToggledEvent(ModeToggled);
            _eventBus.UnsubscribeFromPageChangedEvent(PageChanged);
            _dynamicActionManager.UnregisterAction(this);
            return Task.CompletedTask;
        }

        protected override Task OnSendToPlugin(ActionEventArgs<JObject> args) {
            _settings.Deck = args.Payload["deck"].Value<string>();

            SetSettingsAsync(_settings);

            if (!_initializing) {
                _dynamicActionManager.RefreshAllActions(CardGroupId, ButtonMode);
            }

            return Task.CompletedTask;
        }

        private readonly object _keyUpLock = new object();
        private bool _keyIsDown = false;

        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            _settings = args.Payload.GetSettings<ActionWithDeckSettings>();

            if (_dynamicActionOption != null) {
                //we are showing a menu item, so alert the dynamic action manager instead of our normal behavior
                _dynamicActionManager.OptionSelected(_dynamicActionOption);

                return Task.CompletedTask;
            }


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

                _dynamicActionManager.ShowMenu(this);
            }
        }

        protected override Task OnKeyUp(ActionEventArgs<KeyPayload> args) {
            lock (_keyUpLock) {
                if (!_keyIsDown) {
                    return Task.CompletedTask;
                }
                _keyIsDown = false;
                _keyPressTimer.Enabled = false;

                if (_dynamicActionInfo != default) {
                    _settings = args.Payload.GetSettings<ActionWithDeckSettings>();
                    _eventBus.PublishButtonClickRequest(_dynamicActionInfo.CardGroupId, _dynamicActionInfo.ButtonMode, _dynamicActionInfo.ZoneIndex, _dynamicActionInfo.Index, MouseButton.Left);
                }
            }
            //setting the card name, just because we want the button to update to show the opration is finished (no longer have the "pressed in" look)
            return SetTitleAsync(TextUtils.WrapTitle(_lastSetTitle));
        }

        /// <summary>
        /// Called by the dynamic action manager to make this action display an option instead of its normal text
        /// </summary>
        /// <param name="dynamicActionOption">option to display to ther user, and pass back to the dynamic action manager when the button is pressed</param>
        public void SetOption(DynamicActionOption dynamicActionOption) {
            _dynamicActionOption = dynamicActionOption;
            UpdateButtonToNewDynamicAction();
        }

        /// <summary>
        /// Refresh what information is being displayed on the button update it
        /// </summary>
        /// <param name="dynamicActionInfo">Information to use for updating the display</param>
        public void UpdateButtonToNewDynamicAction(IDynamicActionInfo dynamicActionInfo) {
            if (_dynamicActionOption != null) {
                //we are displaying a menu option, not our normal stuff
                _lastSetTitle = _dynamicActionOption.Text;
                SetTitleAsync(TextUtils.WrapTitle(_dynamicActionOption.Text));
                SetImageAsync(_dynamicActionOption.Image);
                return;
            }

            _dynamicActionInfo = dynamicActionInfo;
            if (_dynamicActionInfo == default) {
                _lastSetTitle = string.Empty;
                SetTitleAsync(string.Empty);
                SetImageAsync(string.Empty);
                return;
            }

            _lastSetTitle = _dynamicActionInfo.Text;
            UpdateButtonDisplay();
        }

        /// <summary>
        /// Refresh what information is being displayed on the button update it
        /// </summary>
        /// <remarks>Retrieves the information since it is not provided</remarks>
        public void UpdateButtonToNewDynamicAction() {
            if (_dynamicActionOption != null) {
                //we are displaying a menu option, not our normal stuff
                _lastSetTitle = _dynamicActionOption.Text;
                SetTitleAsync(TextUtils.WrapTitle(_dynamicActionOption.Text));
                SetImageAsync(_dynamicActionOption.Image);
                return;
            }

            UpdateButtonToNewDynamicAction(_dynamicActionManager.GetInfoForAction(this));
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
            SetButtonMode(ButtonMode == ButtonMode.Pool ? ButtonMode.Zone : ButtonMode.Pool);
        }

        public void SetButtonMode(ButtonMode buttonMode) {
            _page = 0;
            ButtonMode = buttonMode;

            UpdateButtonToNewDynamicAction();
        }

        private void UpdateButtonDisplay() {
            SetTitleAsync(TextUtils.WrapTitle(_lastSetTitle));
            SetImageAsync(_imageService.GetImage(_dynamicActionInfo));
        }
    }
}