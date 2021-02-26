using ArkhamOverlay.Common;
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
            var changedActionInfoList = new List<DynamicActionInfo>();
            lock (_cacheLock) {
                var itemExistsAtLocation = _dynamicActionInfoList.FirstOrDefaultWithContext(buttonContex) != null;
                if (itemExistsAtLocation) {
                    //make room for the item we are inserting by shifting the index of everything else up
                    foreach (var dynamicActioninfo in _dynamicActionInfoList) {
                        if (dynamicActioninfo.IsAtSameIndexOrAfter(buttonContex)) {
                            dynamicActioninfo.Index++;
                            changedActionInfoList.Add(dynamicActioninfo);
                        }
                    }
                }

                var dynamicActionInfo = new DynamicActionInfo(buttonContex);
                dynamicActionInfo.UpdateFromCardInfo(cardInfo);
                _dynamicActionInfoList.Add(dynamicActionInfo);
                changedActionInfoList.Add(dynamicActionInfo);
            }

            PublishChangeEventsForChangedActions(changedActionInfoList);
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
            var changedActionInfoList = new List<DynamicActionInfo>();
            lock (_cacheLock) {
                var dynamicActionToRemove = _dynamicActionInfoList.FirstOrDefaultWithContext(e);
                _dynamicActionInfoList.Remove(dynamicActionToRemove);

                var lastIndex = dynamicActionToRemove.Index;
                foreach (var dynamicActioninfo in _dynamicActionInfoList) {
                    if (dynamicActioninfo.IsAfter(e)) {
                        if (dynamicActioninfo.Index > lastIndex) {
                            lastIndex = dynamicActioninfo.Index;
                        }
                        dynamicActioninfo.Index--;
                        changedActionInfoList.Add(dynamicActioninfo);
                    }
                }

                //we have to add a blank action info at the end so we blank out the previously last item (the list is now shorter)
                var blankDynamicAction = new DynamicActionInfo(e) {
                    Index = lastIndex
                };
                changedActionInfoList.Add(blankDynamicAction);
            }

            PublishChangeEventsForChangedActions(changedActionInfoList);
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
                    _imageService.LoadImage(action.ImageId, action.CardGroupId, action.ButtonMode, action.Index);
                }

                _eventBus.PublishDynamicActionInfoChanged(action);
            }
        }
    }
}