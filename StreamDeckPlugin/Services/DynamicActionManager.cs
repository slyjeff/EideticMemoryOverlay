using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Events;
using StreamDeckPlugin.Actions;
using StreamDeckPlugin.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace StreamDeckPlugin.Services {
    /// <summary>
    /// An option assigned to an dynamic action to display to the user
    /// </summary>
    public class DynamicActionOption {
        /// <summary>
        /// Create DynamicActionOption with a reference to the original dynamic action requesting the menu and the menu option to display
        /// </summary>
        /// <param name="buttonContext">Button context for the button that requires the menu</param>
        /// <param name="option">Option to pass to the right click request if this dynamic action is selected</param>
        /// <param name="text">Text to display for the menu</param>
        /// <param name="image">Image to display for the menu- will use a blank image if null or empty</param>
        public DynamicActionOption(IButtonContext buttonContext, ButtonOption option = null, string text = null, string image = null) {
            ButtonContext = buttonContext;
            Option = option;
            Text = text;
            Image = string.IsNullOrEmpty(image) ? ImageUtils.BlankImage() : image;
        }

        /// <summary>
        /// Button context for the button that requires the menu
        /// </summary>
        public IButtonContext ButtonContext{ get; }

        /// <summary>
        /// Option to pass to the right click request if this dynamic action is selected
        /// </summary>
        public ButtonOption Option { get; }

        /// <summary>
        /// Text to display for the menu
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Image to display for the menu
        /// </summary>
        public string Image { get; }
    }

    /// <summary>
    /// Keep track of all the dynamic buttons so they can be managed as a group
    /// </summary>
    public interface IDynamicActionManager {
        /// <summary>
        /// Start tracking and updating an action's index so it can correctly know what action it correlates to
        /// </summary>
        /// <param name="dynamicAction">A Dynamic Action to track</param>
        /// <remarks>Will not add if it already is being tracked</remarks>
        void RegisterAction(DynamicAction dynamicAction);

        /// <summary>
        /// Stop tracking and updating an action's index
        /// </summary>
        /// <param name="dynamicAction">Dynamic Action to stop tracking</param>
        void UnregisterAction(DynamicAction dynamicAction);

        /// <summary>
        /// Reveluate all indexes for a card group make sure they are correct, updating the index of dynamic actions that are incorrect
        /// </summary>
        /// <remarks>This should be called whenever a dynamic action changes its card group</remarks>
        void ReclaculateIndexes();

        /// <summary>
        /// Displays a menu, or executes the action if there is only one
        /// </summary>
        /// <param name="buttonContext">Button Context used to determine what the available options are</param>
        void ShowMenu(IButtonContext buttonContext);

        /// <summary>
        /// Execute the logic appropriate for the selected option
        /// </summary>
        /// <param name="dynamicActionOption">Originally received from the dynamic action manager; contains the information necessary to handle this menu item</param>
        void OptionSelected(DynamicActionOption dynamicActionOption);
    }

    /// <summary>
    /// Keep track of all the dynamic buttons so they can be managed as a group
    /// </summary>
    public class DynamicActionManager : IDynamicActionManager, IButtonOptionResolver {
        private readonly IList<DynamicAction> _dynamicActions = new List<DynamicAction>();
        private readonly object _dynamicActionsLock = new object();
        private readonly IEventBus _eventBus;
        private readonly IDynamicActionInfoStore _dynamicActionInfoStore;
        private readonly ICardGroupStore _cardGroupStore;
        private readonly IImageService _imageService;
        private bool _recalculateInProgress = false;

        public DynamicActionManager(IEventBus eventBus, IDynamicActionInfoStore dynamicActionInfoStore, ICardGroupStore cardGroupStore, IImageService imageService) {
            _eventBus = eventBus;
            _dynamicActionInfoStore = dynamicActionInfoStore;
            _cardGroupStore = cardGroupStore;
            _imageService = imageService;
        }

        /// <summary>
        /// Start tracking and updating an action's index so it can correctly know what action it correlates to
        /// </summary>
        /// <param name="dynamicAction">A Dynamic Action to track</param>
        /// <remarks>Will not add if it already is being tracked</remarks>
        public void RegisterAction(DynamicAction dynamicAction) {
            lock (_dynamicActionsLock) {
                if (_dynamicActions.Contains(dynamicAction)) {
                    return;
                }

                _dynamicActions.Add(dynamicAction);
            }

            //add a delay so we can gather all the information, then display the buttons
            if (_recalculateInProgress) {
                return;
            }
            _recalculateInProgress = true;

            var delayRecalculateTimer = new Timer(50);
            delayRecalculateTimer.Elapsed += (s, e) => {
                delayRecalculateTimer.Enabled = false;
                _recalculateInProgress = false;
                ReclaculateIndexes();
            };
            delayRecalculateTimer.Enabled = true;
        }

        /// <summary>
        /// Stop tracking and updating an action's index
        /// </summary>
        /// <param name="dynamicAction">A Dynamic Action to track</param>
        /// <remarks>will not add if it already is being tracked</remarks>
        public void UnregisterAction(DynamicAction dynamicAction) {
            lock (_dynamicActionsLock) {
                _dynamicActions.Remove(dynamicAction);
            }
        }

        /// <summary>
        /// Reveluate all indexes for a card group make sure they are correct, updating the index of dynamic actions that are incorrect
        /// </summary>
        /// <remarks>This should be called whenever a dynamic action changes its card group</remarks>
        public void ReclaculateIndexes() {
            if (_recalculateInProgress) {
                return;
            }

            lock (_dynamicActionsLock) {
                foreach (var cardGroupId in EnumUtil.GetValues<CardGroupId>()) {
                    var actionsInCardGroup = GetActionsForCardGroup(cardGroupId).ToList();

                    var relativeIndex = 0;
                    var dynamicActionCount = actionsInCardGroup.Count();
                    foreach (var action in actionsInCardGroup) {
                        action.UpdateIndexInformation(relativeIndex++, dynamicActionCount);
                    }
                }
            }
        }

        /// <summary>
        /// Displays a menu, or executes the action if there is only one
        /// </summary>
        /// <param name="buttonContext">Button Context used to determine what the available options are</param>
        public void ShowMenu(IButtonContext buttonContext) {
            var dynamicActionInfo = _dynamicActionInfoStore.GetDynamicActionInfo(buttonContext);
            if (dynamicActionInfo == null || !dynamicActionInfo.ButtonOptions.Any()) {
                return;
            }

            if (dynamicActionInfo.ButtonOptions.Count == 1) {
                _eventBus.PublishButtonClickRequest(buttonContext.CardGroupId, buttonContext.ButtonMode, buttonContext.Index, MouseButton.Right, dynamicActionInfo.ButtonOptions.First());
                return;
            }

            lock (_dynamicActionsLock) {
                var options = CreateDynamicActionOptions(dynamicActionInfo);
                var optionIndex = 0;
                foreach (var action in GetActionsForCardGroup(buttonContext.CardGroupId)) {
                    if (optionIndex < options.Count) {
                        action.SetOption(options[optionIndex++]);
                        continue;
                    }

                    if (optionIndex == options.Count) {
                        //show a cancel button
                        action.SetOption(new DynamicActionOption(buttonContext, null, "Cancel"));
                        optionIndex++;
                        continue;
                    }

                    //show a blank screen (these buttons will still cancel)
                    action.SetOption(new DynamicActionOption(buttonContext));
                }
            }
        }

        /// <summary>
        /// Execute the logic appropriate for the selected option
        /// </summary>
        /// <param name="dynamicActionOption">Originally received from the dynamic action manager; contains the information necessary to handle this menu item</param>
        public void OptionSelected(DynamicActionOption dynamicActionOption) {
            var buttonContext = dynamicActionOption.ButtonContext;

            //return dynamic actions back to their normal mode
            lock (_dynamicActionsLock) {
                foreach (var action in GetActionsForCardGroup(buttonContext.CardGroupId)) {
                    action.SetOption(null);
                }
            }

            var option = dynamicActionOption.Option;
            if (option == null) {
                return;
            }

            _eventBus.PublishButtonClickRequest(buttonContext.CardGroupId, buttonContext.ButtonMode, buttonContext.Index, MouseButton.Right, option);
        }

        /// <summary>
        /// Create a list of dynamic action options for the specificed dynamic action
        /// </summary>
        /// <param name="dynamicActionInfo">Creat options based on this information</param>
        /// <returns>A list of dynamic action options including dynamicly resolved actions and images if applicable</returns>
        private IList<DynamicActionOption> CreateDynamicActionOptions(IDynamicActionInfo dynamicActionInfo) {
            var options = new List<DynamicActionOption>();
            foreach (var buttonOption in dynamicActionInfo.ButtonOptions) {
                var option = CreateDynamicActionOption(buttonOption, dynamicActionInfo);
                if (option == null) {
                    continue;
                }

                options.Add(option);
            }

            return options;
        }

        /// <summary>
        /// Create a dynamic action option, resolving placeholders from a button option
        /// </summary>
        /// <param name="buttonOption">The button option that is the basis for this dynamic action option</param>
        /// <param name="buttonContext">The button context that is the source of the menu</param>
        /// <returns>A Dynamic action option with resovled text and possibly an image, if appicable</returns>
        private DynamicActionOption CreateDynamicActionOption(ButtonOption buttonOption, IButtonContext buttonContext) {
            var text = buttonOption.GetText(this);
            if (string.IsNullOrEmpty(text)) {
                return default;
            }

            var image = _imageService.GetImage(buttonOption.GetImageId(this));

            return new DynamicActionOption(buttonContext, buttonOption, text, image);
        }

        /// <summary>
        /// Get all dynamic actions for a card group, ordered by physical id
        /// </summary>
        /// <param name="cardGroupId">Get all dynamic actions for this card group</param>
        /// <returns>All dynamic actions for the card group, ordered by physical id</returns>
        private IEnumerable<DynamicAction> GetActionsForCardGroup(CardGroupId cardGroupId) {
            return from dynamicAction in _dynamicActions
                   where dynamicAction.CardGroupId == cardGroupId
                   orderby dynamicAction.PhysicalIndex
                   select dynamicAction;
        }

        /// <summary>
        /// Used by button option to get a name for the card group when displaying an option
        /// </summary>
        /// <param name="cardGroupId">Card group to resolve</param>
        /// <returns>The name of the card group</returns>
        string IButtonOptionResolver.GetCardGroupName(CardGroupId cardGroupId) {
            var cardGroupInfo = _cardGroupStore.GetCardGroupInfo(cardGroupId);
            return cardGroupInfo.Name;
        }

        /// <summary>
        /// Used by button option to get a name for the card zone when displaying an option
        /// </summary>
        /// <param name="cardGroupId">Card group of the card zone</param>
        /// <param name="zoneIndex">Zone to resolve</param>
        /// <returns>Name of the zone</returns>
        string IButtonOptionResolver.GetCardZoneName(CardGroupId cardGroupId, int zoneIndex) {
            var cardGroupInfo = _cardGroupStore.GetCardGroupInfo(cardGroupId);
            if (cardGroupInfo == default) {
                return string.Empty;
            }

            if (zoneIndex >= cardGroupInfo.Zones.Count) {
                return string.Empty;
            }

            return cardGroupInfo.Zones[zoneIndex];
        }

        /// <summary>
        /// Used by button option to get the image ID for a button when displaying an option
        /// </summary>
        /// <param name="cardGroupId">Card group of the card zone</param>
        /// <param name="zoneIndex">Zone to resolve</param>
        /// <returns>Image Id for the card group</returns>
        string IButtonOptionResolver.GetImageId(CardGroupId cardGroupId, int zoneIndex) {
            var cardGroupInfo = _cardGroupStore.GetCardGroupInfo(cardGroupId);
            if (cardGroupInfo != default) {
                return cardGroupInfo.ImageId;
            }
            return string.Empty;
        }
    }
}
