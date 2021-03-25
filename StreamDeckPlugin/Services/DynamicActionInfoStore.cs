using ArkhamOverlay.Common;
using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Events;
using StreamDeckPlugin.Events;
using System.Collections.Generic;
using System.Linq;

namespace StreamDeckPlugin.Services {
    public interface IDynamicActionInfoStore {
        void UpdateDynamicActionInfo(IButtonContext buttonContext, ICardInfo cardInfo);
        IDynamicActionInfo GetDynamicActionInfo(IButtonContext buttonContext);
        IEnumerable<IDynamicActionInfo> GetDynamicActionInfoForGroup(CardGroupId cardGroupId);
    }

    public class DynamicActionInfoStore : IDynamicActionInfoStore {
        private readonly object _cacheLock = new object();
        private readonly List<DynamicActionInfo> _dynamicActionInfoList = new List<DynamicActionInfo>();

        private readonly IEventBus _eventBus;
        private readonly IImageService _imageService;

        public DynamicActionInfoStore(IEventBus eventBus, IImageService imageService) {
            _eventBus = eventBus;
            _imageService = imageService;

            eventBus.SubscribeToCardGroupButtonsChanged(CardGroupButtonsChangedHandler);
            eventBus.SubscribeToImageLoadedEvent(e => ImageLoaded(e.ImageId));
            eventBus.SubscribeToCardInfoVisibilityChanged(e => CardInfoVisibilityChanged(e.Code, e.IsVisible));
            eventBus.SubscribeToButtonInfoChanged(ButtonInfoChanged);
            eventBus.SubscribeToButtonRemoved(ButtonRemoved);
            eventBus.SubscribeToButtonTextChanged(ButtonTextChanged);
            eventBus.SubscribeToButtonToggled(ButtonToggled);
        }

        public IDynamicActionInfo GetDynamicActionInfo(IButtonContext buttonContext) {
            lock (_cacheLock) {
                return _dynamicActionInfoList.FirstOrDefaultWithContext(buttonContext);
            }
        }

        public void UpdateDynamicActionInfo(IButtonContext buttonContex, ICardInfo cardInfo) {
            DynamicActionInfo dynamicActionInfo;
            lock (_cacheLock) {
                dynamicActionInfo = _dynamicActionInfoList.FirstOrDefaultWithContext(buttonContex);
                if (dynamicActionInfo != null) {
                    if (!dynamicActionInfo.CardInfoHasChanged(cardInfo)) {
                        return;
                    }
                    dynamicActionInfo.UpdateFromCardInfo(cardInfo);
                }
            }

            if (dynamicActionInfo == null) {
                AddDynamicActionInfo(buttonContex, cardInfo);
            } else {
                PublishChangeEventsForChangedActions(dynamicActionInfo);
            }
        }

        public void AddDynamicActionInfo(IButtonContext buttonContex, ICardInfo cardInfo) {
            DynamicActionInfo changedAction;
            lock (_cacheLock) {
                var itemExistsAtLocation = _dynamicActionInfoList.FirstOrDefaultWithContext(buttonContex) != null;
                if (itemExistsAtLocation) {
                    //make room for the item we are inserting by shifting the index of everything else up
                    foreach (var dynamicActioninfo in _dynamicActionInfoList) {
                        if (dynamicActioninfo.IsAtSameIndexOrAfter(buttonContex)) {
                            dynamicActioninfo.Index++;
                        }
                    }
                }

                changedAction = new DynamicActionInfo(buttonContex);
                changedAction.UpdateFromCardInfo(cardInfo);
                _dynamicActionInfoList.Add(changedAction);
            }

            //we only have to change the one action that changed, as the handler is smart enough to refresh everything else around
            PublishChangeEventsForChangedActions(changedAction);
        }

        public IEnumerable<IDynamicActionInfo> GetDynamicActionInfoForGroup(CardGroupId cardGroupId) {
            lock (_cacheLock) {
                return _dynamicActionInfoList.Where(x => x.CardGroupId == cardGroupId).ToList();
            }
        }

        /// <summary>
        /// Clear the current list of buttons for this card group and replace it with a new list
        /// </summary>
        /// <param name="eventData">Event data containing list of new buttons</param>
        private void CardGroupButtonsChangedHandler(CardGroupButtonsChanged eventData) {
            IList<DynamicActionInfo> actionsUpdated = new List<DynamicActionInfo>();

            lock (_cacheLock) {
                var removedActions = _dynamicActionInfoList.Where(x => x.CardGroupId == eventData.CardGroupId).ToList();

                _dynamicActionInfoList.RemoveAll(x => x.CardGroupId == eventData.CardGroupId);

                foreach (var button in eventData.Buttons) {
                    var changedAction = new DynamicActionInfo(button);
                    changedAction.UpdateFromCardInfo(button);
                    _dynamicActionInfoList.Add(changedAction);
                    actionsUpdated.Add(changedAction);
                }

                //if we removed in pool buttons that were not replaced, we need to issue an update for them
                foreach (var removedAction in removedActions) {
                    if (removedAction.ButtonMode != ButtonMode.Pool) {
                        continue;
                    }

                    if (actionsUpdated.FirstOrDefaultWithContext(removedAction) != default) {
                        continue;
                    }

                    actionsUpdated.Add(new DynamicActionInfo(removedAction));
                }
            }

            PublishChangeEventsForChangedActions(actionsUpdated);
        }

        private void ImageLoaded(string imageId) {
            var dynamicActionInfoWithImageItems = new List<DynamicActionInfo>();
            lock (_cacheLock) {
                dynamicActionInfoWithImageItems = (from dynamicActionInfo in _dynamicActionInfoList
                                                  where dynamicActionInfo.ImageId == imageId
                                                  select dynamicActionInfo).ToList();
            }

            PublishChangeEventsForChangedActions(dynamicActionInfoWithImageItems);
        }

        private void CardInfoVisibilityChanged(string code, bool isVisible) {
            var changedActionInfoList = new List<DynamicActionInfo>();
            lock (_cacheLock) {
                foreach (var dynamicActionInfo in _dynamicActionInfoList) {
                    if (dynamicActionInfo.ImageId == code && dynamicActionInfo.IsToggled != isVisible) {
                        dynamicActionInfo.IsToggled = isVisible;
                        changedActionInfoList.Add(dynamicActionInfo);
                    }
                }
            }

            PublishChangeEventsForChangedActions(changedActionInfoList);
        }

        private void ButtonInfoChanged(ButtonInfoChanged eventData) {
            if (eventData.Action == ChangeAction.Update) {
                UpdateDynamicActionInfo(eventData, eventData);
            } else {
                AddDynamicActionInfo(eventData, eventData);
            }
        }

        private void ButtonRemoved(ButtonRemoved e) {
            DynamicActionInfo removedActionInfo;
            lock (_cacheLock) {
                removedActionInfo = _dynamicActionInfoList.FirstOrDefaultWithContext(e);
                if (removedActionInfo == null) {
                    return;
                }
                _dynamicActionInfoList.Remove(removedActionInfo);

                var lastIndex = removedActionInfo.Index;
                foreach (var dynamicActioninfo in _dynamicActionInfoList) {
                    if (dynamicActioninfo.IsAfter(e)) {
                        if (dynamicActioninfo.Index > lastIndex) {
                            lastIndex = dynamicActioninfo.Index;
                        }
                        dynamicActioninfo.Index--;
                    }
                }
            }

            //we only have to change the one action that changed, as the handler is smart enough to refresh everything else around
            PublishChangeEventsForChangedActions(removedActionInfo);
        }

        private void ButtonTextChanged(ButtonTextChanged e) {
            IList<DynamicActionInfo> dynamicActionsToChange = new List<DynamicActionInfo>();
            lock (_cacheLock) {
                foreach (var dynamicActionInfo in _dynamicActionInfoList.FindAllWithContext(e)) {
                    dynamicActionInfo.Text = e.Text;
                    dynamicActionsToChange.Add(dynamicActionInfo);
                }
            }

            PublishChangeEventsForChangedActions(dynamicActionsToChange);
        }

        private void ButtonToggled(ButtonToggled e) {
            IList<DynamicActionInfo> dynamicActionsToChange = new List<DynamicActionInfo>();
            lock (_cacheLock) {
                foreach (var dynamicActionInfo in _dynamicActionInfoList.FindAllWithContext(e)) {
                    dynamicActionInfo.IsToggled = e.IsToggled;
                    dynamicActionsToChange.Add(dynamicActionInfo);
                }
            }

            PublishChangeEventsForChangedActions(dynamicActionsToChange);
        }

        private void PublishChangeEventsForChangedActions(DynamicActionInfo action) {
            PublishChangeEventsForChangedActions(new List<DynamicActionInfo> { action });
        }

        private void PublishChangeEventsForChangedActions(IEnumerable<DynamicActionInfo> changedActions) {
            foreach (var action in changedActions) {
                if (action.IsImageAvailable) {
                    _imageService.LoadImage(action.ImageId, action.CardGroupId, action.ButtonMode, action.ZoneIndex, action.Index);
                }

                _eventBus.PublishDynamicActionInfoChanged(action);
            }
        }
    }
}