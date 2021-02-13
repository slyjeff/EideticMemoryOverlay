using ArkhamOverlay.Common;
using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
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

        public DynamicActionInfoStore(IEventBus eventBus) {
            _eventBus = eventBus;
            eventBus.SubscribeToImageLoadedEvent(e => ImageLoaded(e.ImageId));
            eventBus.SubscribeToCardTemplateVisibilityChanged(e => CardTemplateVisibilityChanged(e.Name, e.IsVisible));
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
                //don't raise events within a lock
                _eventBus.PublishDynamicActionInfoChanged(dynamicActionInfo);
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

            //don't raise events within a lock
            foreach (var dynamicActionInfo in changedActionInfoList) {
                _eventBus.PublishDynamicActionInfoChanged(dynamicActionInfo);
            }
        }


        private void ImageLoaded(string imageId) {
            var dynamicActionInfoWithImageItems = new List<DynamicActionInfo>();
            lock (_cacheLock) {
                dynamicActionInfoWithImageItems = (from dynamicActionInfo in _dynamicActionInfoList
                                                  where dynamicActionInfo.ImageId == imageId
                                                  select dynamicActionInfo).ToList();
            }

            foreach (var dynamicActionInfo in dynamicActionInfoWithImageItems) {
                _eventBus.PublishDynamicActionInfoChanged(dynamicActionInfo);
            }
        }

        private void CardTemplateVisibilityChanged(string name, bool isVisible) {
            var changedActionInfoList = new List<DynamicActionInfo>();
            lock (_cacheLock) {
                foreach (var dynamicActionInfo in _dynamicActionInfoList) {
                    if (dynamicActionInfo.ImageId == name && dynamicActionInfo.IsToggled != isVisible) {
                        dynamicActionInfo.IsToggled = isVisible;
                        changedActionInfoList.Add(dynamicActionInfo);
                    }
                }
            }

            //don't raise events within a lock
            foreach (var dynamicActionInfo in changedActionInfoList) {
                _eventBus.PublishDynamicActionInfoChanged(dynamicActionInfo);
            }
        }

        private void ButtonInfoChanged(ButtonInfoChanged e) {
            if (e.Action == ChangeAction.Update) {
                UpdateDynamicActionInfo(e, e);
            } else {
                AddDynamicActionInfo(e, e);
            }
        }

        private void ButtonRemoved(ButtonRemoved e) {
            var changedActionInfoList = new List<DynamicActionInfo>();
            lock (_cacheLock) {
                var dynamicActionToRemove = _dynamicActionInfoList.FirstOrDefaultWithContext(e);
                //changedActionInfoList.Add(dynamicActionToRemove);
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

            //don't raise events within a lock
            foreach (var dynamicActionInfo in changedActionInfoList) {
                _eventBus.PublishDynamicActionInfoChanged(dynamicActionInfo);
            }
        }

        private void ButtonTextChanged(ButtonTextChanged e) {
            IList<DynamicActionInfo> dynamicActionsToChange = new List<DynamicActionInfo>();
            lock (_cacheLock) {
                foreach (var dynamicActionInfo in _dynamicActionInfoList.FindAllWithContext(e)) {
                    dynamicActionInfo.Text = e.Text;
                    dynamicActionsToChange.Add(dynamicActionInfo);
                }
            }

            //don't raise events within a lock
            foreach (var dynamicActionInfo in dynamicActionsToChange) {
                _eventBus.PublishDynamicActionInfoChanged(dynamicActionInfo);
            }
        }

        private void ButtonToggled(ButtonToggled e) {
            IList<DynamicActionInfo> dynamicActionsToChange = new List<DynamicActionInfo>();
            lock (_cacheLock) {
                foreach (var dynamicActionInfo in _dynamicActionInfoList.FindAllWithContext(e)) {
                    dynamicActionInfo.IsToggled = e.IsToggled;
                    dynamicActionsToChange.Add(dynamicActionInfo);
                }
            }

            //don't raise events within a lock
            foreach (var dynamicActionInfo in dynamicActionsToChange) {
                _eventBus.PublishDynamicActionInfoChanged(dynamicActionInfo);
            }
        }
    }
}