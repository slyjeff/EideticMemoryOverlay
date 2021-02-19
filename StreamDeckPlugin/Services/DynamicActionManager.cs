using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Events;
using StreamDeckPlugin.Actions;
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
        /// <param name="text">Text to displaay for the menu</param>
        public DynamicActionOption(IButtonContext buttonContext, string option, string text) {
            ButtonContext = buttonContext;
            Option = option;
            Text = text;
        }

        /// <summary>
        /// Button context for the button that requires the menu
        /// </summary>
        public IButtonContext ButtonContext{ get; }

        /// <summary>
        /// Option to pass to the right click request if this dynamic action is selected
        /// </summary>
        public string Option { get; }

        /// <summary>
        /// Text to displaay for the menu
        /// </summary>
        public string Text { get; }
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
        /// If there is more than one option based on this button context, use dynamic actions to display a menu of options, and then publish and event with the chosen option
        /// </summary>
        /// <param name="buttonContext">Button Context used to determine if a menu needs to be shown (more than one option is available)</param>
        /// <returns>Whether a menu is necessary</returns>
        bool ShowMenuIfNecessary(IButtonContext buttonContext);

        /// <summary>
        /// Execute the logic appropriate for the selected option
        /// </summary>
        /// <param name="dynamicActionOption">Originally received from the dynamic action manager; contains the information necessary to handle this menu item</param>
        void OptionSelected(DynamicActionOption dynamicActionOption);
    }

    /// <summary>
    /// Keep track of all the dynamic buttons so they can be managed as a group
    /// </summary>
    public class DynamicActionManager : IDynamicActionManager {
        private readonly IList<DynamicAction> _dynamicActions = new List<DynamicAction>();
        private readonly object _dynamicActionsLock = new object();
        private readonly IEventBus _eventBus;
        private readonly IDynamicActionInfoStore _dynamicActionInfoStore;
        private bool _recalculateInProgress = false;

        public DynamicActionManager(IEventBus eventBus, IDynamicActionInfoStore dynamicActionInfoStore) {
            _eventBus = eventBus;
            _dynamicActionInfoStore = dynamicActionInfoStore;
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
        /// Displays a menu if necessary
        /// </summary>
        /// <param name="buttonContext">Button Context used to determine if a menu needs to be shown (more than one option is available)</param>
        /// <returns>Whether a menu is necessary</returns>
        public bool ShowMenuIfNecessary(IButtonContext buttonContext) {
            var dynamicActionInfo = _dynamicActionInfoStore.GetDynamicActionInfo(buttonContext);
            if (dynamicActionInfo == null || dynamicActionInfo.ButtonOptions.Count <= 1) {
                return false;
            }
            var buttonOptions = dynamicActionInfo.ButtonOptions;

            lock (_dynamicActionsLock) {
                var buttonOptionIndex = 0;
                foreach (var action in GetActionsForCardGroup(buttonContext.CardGroupId)) {
                    if (buttonOptionIndex < buttonOptions.Count) {
                        var option = buttonOptions[buttonOptionIndex++];
                        var text = option.GetTextResolvingPlaceholders(ResolveOptionPlaceholder);

                        action.SetOption(new DynamicActionOption(buttonContext, option.Option, text));
                        continue;
                    }

                    if (buttonOptionIndex == buttonOptions.Count) {
                        //show a cancel button
                        action.SetOption(new DynamicActionOption(buttonContext, string.Empty, "Cancel"));
                        buttonOptionIndex++;
                        continue;
                    }

                    //show a blank screen (these buttons will still cancel)
                    action.SetOption(new DynamicActionOption(buttonContext, string.Empty, string.Empty));
                }
            }

            return true;
        }

        /// <summary>
        /// Callback to resolve placeholder in a menu item so we can provide more contextual information
        /// </summary>
        /// <param name="placeholder">The placeholder to resolve</param>
        /// <returns>The actual value the placeholder represents</returns>
        /// <remarks>Example "player1" (represented by <<xxxx>></xxxx> in the text) will resolve to the name of player 1 in game data</remarks>
        private string ResolveOptionPlaceholder(string parameterName) {
            return parameterName;
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
            if (string.IsNullOrEmpty(option)) {
                return;
            }

            _eventBus.PublishButtonClickRequest(buttonContext.CardGroupId, buttonContext.ButtonMode, buttonContext.Index, MouseButton.Right, option);
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
    }
}
