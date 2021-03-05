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
        private readonly IList<DynamicActionInfo> _dynamicActionInfoList = new List<DynamicActionInfo>();

        private readonly IEventBus _eventBus;
        private readonly IImageService _imageService;

        public DynamicActionInfoStore(IEventBus eventBus, IImageService imageService) {
            _eventBus = eventBus;
            _imageService = imageService;

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
                return (from dynamicActionInfo in _dynamicActionInfoList
                        where dynamicActionInfo.CardGroupId == cardGroupId
                        select dynamicActionInfo).ToList();
            }
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