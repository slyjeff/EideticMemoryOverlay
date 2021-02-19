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
    public class DynamicAction : StreamDeckAction<ActionWithDeckSettings>, IButtonContext {
        private readonly IDynamicActionInfoStore _dynamicActionInfoStore = ServiceLocator.GetService<IDynamicActionInfoStore>();
        private readonly IDynamicActionManager _dynamicActionManager = ServiceLocator.GetService<IDynamicActionManager>();
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        private readonly IImageService _imageService = ServiceLocator.GetService<IImageService>();

        private Coordinates _coordinates = new Coordinates();
        private ActionWithDeckSettings _settings = new ActionWithDeckSettings();

        private string _deviceId;
        private readonly Timer _keyPressTimer = new Timer(700);
        private string _lastSetTitle;
        private int _page;
        private DynamicActionOption _dynamicActionOption;


        public DynamicAction() {
            _keyPressTimer.Enabled = false;
            _keyPressTimer.Elapsed += KeyHeldDown;
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

        private int _relativeIndex = -1;
        public int _dynamicActionCount = 0;

        /// <summary>
        /// Index of the Button in the UI this Dynamic Action corresponds to
        /// </summary>
        /// <remarks>Takes into account the Relative Index as well as the page</remarks>
        public int Index { get { return (_page * _dynamicActionCount) + _relativeIndex; } }

        protected override Task OnWillAppear(ActionEventArgs<AppearancePayload> args) {
            _coordinates = args.Payload.Coordinates;
            _deviceId = args.Device;
            _settings = args.Payload.GetSettings<ActionWithDeckSettings>();

            _dynamicActionManager.RegisterAction(this);

            _eventBus.SubscribeToDynamicActionInfoChangedEvent(DynamicActionChanged);
            _eventBus.SubscribeToPageChangedEvent(PageChanged);
            _eventBus.SubscribeToModeToggledEvent(ModeToggled);

            SetButtonMode(ButtonMode.Pool);
 
            return Task.CompletedTask;
        }

        protected override Task OnWillDisappear(ActionEventArgs<AppearancePayload> args) {
            _eventBus.UnsubscribeFromModeToggledEvent(ModeToggled);
            _eventBus.UnsubscribeFromPageChangedEvent(PageChanged);
            _eventBus.UnsubscribeFromDynamicActionInfoChangedEvent(DynamicActionChanged);
            _dynamicActionManager.UnregisterAction(this);
            return Task.CompletedTask;
        }

        protected override Task OnSendToPlugin(ActionEventArgs<JObject> args) {
            _settings.Deck = args.Payload["deck"].Value<string>();

            _dynamicActionManager.ReclaculateIndexes();

            SetSettingsAsync(_settings);

            GetButtonInfo();

            return Task.CompletedTask;
        }

        private object _keyUpLock = new object();
        private bool _keyIsDown = false;

        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args) {
            _settings = args.Payload.GetSettings<ActionWithDeckSettings>();

            if (_dynamicActionOption != null) {
                //we are showing a menyu item, so alert the dynamic action manager instead of our normal behavior
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

                if (_dynamicActionManager.ShowMenuIfNecessary(this)) {
                    return;
                }
                SendClick(MouseButton.Right);
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
                SendClick(MouseButton.Left);

                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Called by the dynamic action manager to set information necessary for calculating its index
        /// </summary>
        /// <param name="relativeIndex">The index of the Dynamic Action relative to all other Dynamic Actions assigned to the same Card Group</param>
        /// <param name="">The total number of Dynamic Actions in this Dynamic Action's Card Group</param>
        public void UpdateIndexInformation(int relativeIndex, int dynamicActionCount) {
            var logicalIndexBeforeUpdate = Index;
            _relativeIndex = relativeIndex;
            _dynamicActionCount = dynamicActionCount;

            //don't request new information if our index hasn't changed
            if (Index != logicalIndexBeforeUpdate) {
                UpdateButtonToNewDynamicAction();
            }
        }

        /// <summary>
        /// Called by the dynamic action manager to make this action display an option instead of its normal text
        /// </summary>
        /// <param name="dynamicActionOption">option to display to ther user, and pass back to the dynamic action manager when the button is pressed</param>
        public void SetOption(DynamicActionOption dynamicActionOption) {
            _dynamicActionOption = dynamicActionOption;
            UpdateButtonToNewDynamicAction();
        }

        private void SendClick(MouseButton mouseButton) {
            _eventBus.PublishButtonClickRequest(CardGroupId, ButtonMode, Index, mouseButton, string.Empty);

            //setting the card name, just because we want the button to update to show the opration is finished (no longer have the "pressed in" look)
            SetTitleAsync(TextUtils.WrapTitle(_lastSetTitle));
        }

        private void GetButtonInfo() {
            _eventBus.PublishGetButtonInfoRequest(CardGroupId, ButtonMode, Index);
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
            SetButtonMode(ButtonMode == ButtonMode.Pool ? ButtonMode.Zone : ButtonMode.Pool);
        }

        public void SetButtonMode(ButtonMode buttonMode) {
            _page = 0;
            ButtonMode = buttonMode;

            UpdateButtonToNewDynamicAction();
        }

        private void UpdateButtonToNewDynamicAction() {
            if (_relativeIndex == -1) {
                //we don't know our position yet, so don't try to display anything
                return;
            }

            if (_dynamicActionOption != null) {
                //we are displaying a menu option, not our normal stuff
                SetTitleAsync(TextUtils.WrapTitle(_dynamicActionOption.Text));
                SetImageAsync(_dynamicActionOption.Image);
                return;
            }

            var dynamicActionInfo = _dynamicActionInfoStore.GetDynamicActionInfo(this);
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
            SetImageAsync(_imageService.GetImage(dynamicActionInfo));
        }
    }
}