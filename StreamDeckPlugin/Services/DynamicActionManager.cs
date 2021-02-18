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
        /// <param name="buttonOption">The original dynamic action requesting the menu</param>
        /// <param name="action">The menu option to display</param>
        public DynamicActionOption(ButtonOption buttonOption, DynamicAction action) {
            ButtonOption = buttonOption;
            Action = action;
        }

        /// <summary>
        /// The original dynamic action requesting the menu
        /// </summary>
        public DynamicAction Action { get; }

        /// <summary>
        /// The menu option to display
        /// </summary>
        public ButtonOption ButtonOption { get; }
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
        /// Use dynamic actions to display a menu of options, and then publish and event with the chosen option
        /// </summary>
        /// <param name="dynamicAction">Action that is initiating this menu</param>
        /// <param name="buttonOptions">List of options to present to the user</param>
        void ShowMenu(DynamicAction dynamicAction, IList<ButtonOption> buttonOptions);

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
        private bool _recalculateInProgress = false;

        public DynamicActionManager(IEventBus eventBus) {
            _eventBus = eventBus;
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
        /// Use dynamic actions to display a menu of options, and then publish and event with the chosen option
        /// </summary>
        /// <param name="dynamicAction">Action that is initiating this menu</param>
        /// <param name="buttonOptions">List of options to present to the user</param>
        public void ShowMenu(DynamicAction dynamicAction, IList<ButtonOption> buttonOptions) {
            lock (_dynamicActionsLock) {
                var buttonOptionIndex = 0;
                foreach (var action in GetActionsForCardGroup(dynamicAction.CardGroupId)) {
                    if (buttonOptionIndex < buttonOptions.Count) {
                        action.SetOption(new DynamicActionOption(buttonOptions[buttonOptionIndex++], dynamicAction));
                        continue;
                    }

                    if (buttonOptionIndex == buttonOptions.Count) {
                        //show a cancel button
                        action.SetOption(new DynamicActionOption(new ButtonOption(string.Empty, "Cancel"), dynamicAction));
                        buttonOptionIndex++;
                        continue;
                    }

                    //show a blank screen (these buttons will still cancel)
                    action.SetOption(new DynamicActionOption(new ButtonOption(string.Empty, string.Empty), dynamicAction));
                }
            }
        }

        /// <summary>
        /// Execute the logic appropriate for the selected option
        /// </summary>
        /// <param name="dynamicActionOption">Originally received from the dynamic action manager; contains the information necessary to handle this menu item</param>
        public void OptionSelected(DynamicActionOption dynamicActionOption) {
            var dynamicAction = dynamicActionOption.Action;

            //return dynamic actions back to their normal mode
            lock (_dynamicActionsLock) {
                foreach (var action in GetActionsForCardGroup(dynamicAction.CardGroupId)) {
                    action.SetOption(null);
                }
            }

            var option = dynamicActionOption.ButtonOption.Option;
            if (string.IsNullOrEmpty(option)) {
                return;
            }

            _eventBus.PublishButtonClickRequest(dynamicAction.CardGroupId, dynamicAction.ButtonMode, dynamicAction.Index, MouseButton.Right, option);
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
