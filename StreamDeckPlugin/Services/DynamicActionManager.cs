using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Events;
using StreamDeckPlugin.Actions;
using StreamDeckPlugin.Events;
using StreamDeckPlugin.Utils;
using System.Collections.Generic;
using System.Linq;

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
        /// Using the dynamic action's positional information and state, find the correct dynamic action info
        /// </summary>
        /// <param name="dynamicAction">Find information for this action</param>
        /// <returns>Information for the dynamic action</returns>
        IDynamicActionInfo GetInfoForAction(DynamicAction dynamicAction);

        /// <summary>
        /// Update all actions to make ssure they show the proper information
        /// </summary>
        void RefreshAllActions();

        /// <summary>
        /// Displays a menu, or executes the action if there is only one
        /// </summary>
        /// <param name="dynamicAction">the dynamic action that was pressed to show the menu</param>
        void ShowMenu(DynamicAction dynamicAction);

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

        public DynamicActionManager(IEventBus eventBus, IDynamicActionInfoStore dynamicActionInfoStore, ICardGroupStore cardGroupStore, IImageService imageService) {
            _eventBus = eventBus;
            _dynamicActionInfoStore = dynamicActionInfoStore;
            _cardGroupStore = cardGroupStore;
            _imageService = imageService;

            _eventBus.SubscribeToDynamicActionInfoChangedEvent(DynamicActionChanged);
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
        /// Update all actions to make ssure they show the proper information
        /// </summary>
        public void RefreshAllActions() {
            IList<DynamicAction> allActions;
            lock (_dynamicActionsLock) {
                allActions = _dynamicActions.ToList();
            }
            
            foreach (var action in allActions) {
                action.UpdateButtonToNewDynamicAction();
            }
        }

        /// <summary>
        /// Using the dynamic action's positional information and state, find the correct dynamic action info
        /// </summary>
        /// <param name="dynamicAction">Find information for this action</param>
        /// <returns>Information for the dynamic action</returns>
        public IDynamicActionInfo GetInfoForAction(DynamicAction dynamicAction) {
            return (dynamicAction.ButtonMode == ButtonMode.Pool)
                ? GetInfoForPoolAction(dynamicAction)
                : GetZoneInfoForZoneAction(dynamicAction);
        }

        /// <summary>
        /// Displays a menu, or executes the action if there is only one
        /// </summary>
        /// <param name="dynamicAction">the dynamic action that was pressed to show the menu</param>
        public void ShowMenu(DynamicAction dynamicAction) {
            var dynamicActionInfo = GetInfoForAction(dynamicAction);
            if (dynamicActionInfo == null || dynamicActionInfo.ButtonOptions == null || !dynamicActionInfo.ButtonOptions.Any()) {
                return;
            }

            if (dynamicActionInfo.ButtonOptions.Count == 1) {
                _eventBus.PublishButtonClickRequest(dynamicActionInfo.CardGroupId, dynamicActionInfo.ButtonMode, dynamicActionInfo.ZoneIndex, dynamicActionInfo.Index, MouseButton.Right, dynamicActionInfo.ButtonOptions.First());
                return;
            }

            lock (_dynamicActionsLock) {
                var options = CreateDynamicActionOptions(dynamicActionInfo);
                var optionIndex = 0;
                foreach (var action in GetActionsForCardGroup(dynamicActionInfo.CardGroupId)) {
                    if (optionIndex < options.Count) {
                        action.SetOption(options[optionIndex++]);
                        continue;
                    }

                    if (optionIndex == options.Count) {
                        //show a cancel button
                        action.SetOption(new DynamicActionOption(dynamicActionInfo, null, "Cancel"));
                        optionIndex++;
                        continue;
                    }

                    //show a blank screen (these buttons will still cancel)
                    action.SetOption(new DynamicActionOption(dynamicActionInfo));
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

            _eventBus.PublishButtonClickRequest(buttonContext.CardGroupId, buttonContext.ButtonMode, buttonContext.ZoneIndex, buttonContext.Index, MouseButton.Right, option);
        }

        /// <summary>
        /// Whenver a dynamic action changes, find it ant notify it to update
        /// </summary>
        /// <param name="dynamicActionInfoChangedEvent"></param>
        private void DynamicActionChanged(DynamicActionInfoChangedEvent eventData) {
            if (eventData.DynamicActionInfo.ButtonMode == ButtonMode.Pool) {
                PoolDynamicActionChanged(eventData.DynamicActionInfo);
                return;
            }

            //if a zone button has change, just refresh everthing, as often other buttons have to shift
            RefreshAllActions();
        }

        /// <summary>
        /// A pool card button has changed for a card in the pool, so update the dynamic action accordingly
        /// </summary>
        /// <param name="dynamicActionInfo">Info for the action that has changed</param>
        private void PoolDynamicActionChanged(IDynamicActionInfo dynamicActionInfo) {
            IList<DynamicAction> actions;
            lock (_dynamicActionsLock) {
                actions = GetActionsForCardGroup(dynamicActionInfo.CardGroupId).ToList();
            }

            var actionsPerPage = actions.Count;
            if (actionsPerPage == 0) {
                return;
            }
        }

        /// <summary>
        /// A zone card button has changed for a card in the pool, so update the dynamic action accordingly
        /// </summary>
        /// <param name="dynamicActionInfo">Info for the action that has changed</param>
        private void ZoneDynamicActionChanged(IDynamicActionInfo dynamicActionInfo) {
            IList<DynamicAction> actions;
            lock (_dynamicActionsLock) {
                actions = GetActionsForCardGroup(dynamicActionInfo.CardGroupId).ToList();
            }

            var actionsPerPage = actions.Count;
            if (actionsPerPage == 0) {
                return;
            }

            var page = dynamicActionInfo.Index / actionsPerPage;
            var actionIndex = dynamicActionInfo.Index % actionsPerPage;
            if (actions[actionIndex].Page == page) {
                actions[actionIndex].UpdateButtonToNewDynamicAction(dynamicActionInfo);
            }
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
        /// Get dynamic action info for a pool action based on its card group and position
        /// </summary>
        /// <param name="dynamicAction">Find information for this action</param>
        /// <returns>Information for the dynamic action</returns>
        private IDynamicActionInfo GetInfoForPoolAction(DynamicAction dynamicAction) {
            IList<DynamicAction> actions;
            lock (_dynamicActionsLock) {
                actions = GetActionsForCardGroup(dynamicAction.CardGroupId).ToList();
            }

            var relativeIndex = actions.IndexOf(dynamicAction);
            var actionsPerPage = actions.Count;
            var index = dynamicAction.Page * actionsPerPage + relativeIndex;

            var dynamicActionInfo = _dynamicActionInfoStore.GetDynamicActionInfoForGroup(dynamicAction.CardGroupId)
                                        .FirstOrDefault(x => x.ButtonMode == ButtonMode.Pool && x.Index == index);

            if (dynamicActionInfo == default)  {
                _eventBus.PublishGetButtonInfoRequest(dynamicAction.CardGroupId, ButtonMode.Pool, 0, index);
                //try again, now that we've retrieved it
                dynamicActionInfo = _dynamicActionInfoStore.GetDynamicActionInfoForGroup(dynamicAction.CardGroupId)
                                    .FirstOrDefault(x => x.ButtonMode == ButtonMode.Pool && x.Index == index);
            }

            return dynamicActionInfo;
        }

        /// <summary>
        /// Get dynamic action info for a zone action based on its card group and position
        /// </summary>
        /// <param name="dynamicAction">Find information for this action</param>
        /// <returns>Information for the dynamic action</returns>
        private IDynamicActionInfo GetZoneInfoForZoneAction(DynamicAction dynamicAction) {
            IList<DynamicAction> actions;
            lock (_dynamicActionsLock) {
                actions = GetActionsForCardGroup(dynamicAction.CardGroupId).ToList();
            }

            var actionsPerPage = actions.Count;
            var relativeIndex = actions.IndexOf(dynamicAction);
            var pagedRelativeIndex = dynamicAction.Page * actionsPerPage + relativeIndex;

            var dynamicActionInfoList = (from info in _dynamicActionInfoStore.GetDynamicActionInfoForGroup(dynamicAction.CardGroupId)
                                         where info.ButtonMode == ButtonMode.Zone
                                         orderby info.ZoneIndex, info.Index
                                         select info).ToList();

            var actionIndex = 0;
            var lastZoneIndex = 0;
            var lastIndex = 0;

            foreach (var actionInfo in dynamicActionInfoList) {
                if (actionInfo.ZoneIndex > lastZoneIndex) {
                    //we need to find the next physical row
                    actionIndex = FindIndexOfNextPhysicalRow(actions, actionIndex);
                    actionIndex += actionInfo.Index;
                } else {
                    //move to the next action index
                    actionIndex += actionInfo.Index - lastIndex;
                }

                if (actionIndex == pagedRelativeIndex) {
                    //we found it
                    return actionInfo;
                }

                //there is no action info for this button, since we keep some buttons blank in between zones
                if (actionIndex > pagedRelativeIndex) {
                    return default;
                }

                lastZoneIndex = actionInfo.ZoneIndex;
                lastIndex = actionInfo.Index;
            }

            //if we ran through all the info and haven't gotten here, return null, but request the info
            //_eventBus.PublishGetButtonInfoRequest(dynamicAction.CardGroupId, ButtonMode.Zone, zoneIndex, index);
            return default;
        }

        /// <summary>
        /// Find the index of the next row of actions
        /// </summary>
        /// <param name="actions">Find the next row in this list of actions</param>
        /// <param name="startingIndex">Find the next row after the row of the action at this index</param>
        /// <returns>Index of the new row</returns>
        private int FindIndexOfNextPhysicalRow(IList<DynamicAction> actions, int startingIndex) {
            var index = startingIndex;
            var row = actions[index].PhysicalRow;
            var lastColumn = actions[index].PhysicalColumn;
            while (index < actions.Count) {
                if (actions[index].PhysicalRow != row) {
                    //the row changed- we are on a new row
                    return index;
                }

                if (actions[index].PhysicalColumn < lastColumn) {
                    //the column moved back, which means we switched to a new page on the same row
                    return index;
                }

                lastColumn = actions[index].PhysicalColumn;
                index++;
            }
            return actions.Count;
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
