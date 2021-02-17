using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Utils;
using StreamDeckPlugin.Actions;
using System.Collections.Generic;
using System.Linq;

namespace StreamDeckPlugin.Services {
    /// <summary>
    /// Keep track of all the dynamic buttons and calculate their relative indexes to one another, so non-dynamic buttons do not throw them off
    /// </summary>
    public interface IDynamicActionIndexService {
        /// <summary>
        /// Start tracking and updating an action's index so it can correctly know what action it correlates to
        /// </summary>
        /// <param name="dynamicAction">A Dynamic Action to track</param>
        /// <remarks>will not add if it already is being tracked</remarks>
        void RegisterAction(DynamicAction dynamicAction);

        /// <summary>
        /// Reveluate all indexes for a card group make sure they are correct, updating the index of dynamic actions that are incorrect
        /// </summary>
        /// <remarks>This should be called whenever a dynamic action changes its card group</remarks>
        void ReclaculateIndexes();
    }

    /// <summary>
    /// Keep track of all the dynamic buttons and calculate their relative indexes to one another, so non-dynamic buttons do not throw them off
    /// </summary>
    public class DynamicActionIndexService : IDynamicActionIndexService {
        private readonly IList<DynamicAction> _dynamicActions = new List<DynamicAction>();
        private readonly object _dynamicActionsLock = new object();

        /// <summary>
        /// Start tracking and updating an action's index so it can correctly know what action it correlates to
        /// </summary>
        /// <param name="dynamicAction">A Dynamic Action to track</param>
        /// <remarks>will not add if it already is being tracked</remarks>
        public void RegisterAction(DynamicAction dynamicAction) {
            lock (_dynamicActionsLock) {
                if (_dynamicActions.Contains(dynamicAction)) {
                    return;
                }

                _dynamicActions.Add(dynamicAction);
            }
            ReclaculateIndexes();
        }

        /// <summary>
        /// Reveluate all indexes for a card group make sure they are correct, updating the index of dynamic actions that are incorrect
        /// </summary>
        /// <remarks>This should be called whenever a dynamic action changes its card group</remarks>
        public void ReclaculateIndexes() {
            lock (_dynamicActionsLock) {
                foreach (var cardGroupId in EnumUtil.GetValues<CardGroupId>()) {
                    var actionsInCardGroup = from dynamicAction in _dynamicActions
                                             where dynamicAction.CardGroupId == cardGroupId
                                             orderby dynamicAction.PhysicalIndex
                                             select dynamicAction;

                    var relativeIndex = 0;
                    var dynamicActionCount = actionsInCardGroup.Count();
                    foreach (var action in actionsInCardGroup) {
                        action.RelativeIndex = relativeIndex++;
                        action.CountOfDynamicActionsInCardGroup = dynamicActionCount;
                    }
                }
            }
        }
    }
}
