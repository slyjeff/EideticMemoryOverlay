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
            eventBus.SubscribeToCardTemplateVisibilityChangedEvent(e => CardTemplateVisibilityChanged(e.Name, e.IsVisible));
            eventBus.SubscribeToButtonTextChangedEvent(ButtonTextChanged);
            eventBus.SubscribeToButtonToggledEvent(ButtonToggled);
        }

        public IDynamicActionInfo GetDynamicActionInfo(IButtonContext buttonContext) {
            lock (_cacheLock) {
                return _dynamicActionInfoList.FirstOrDefaultWithContext(buttonContext);
            }
        }

        public void UpdateDynamicActionInfo(IButtonContext buttonContex, ICardInfo cardInfo) {
            var changedActionInfoList = new List<DynamicActionInfo>();
            lock (_cacheLock) {
                var dynamicActionInfo = _dynamicActionInfoList.FirstOrDefaultWithContext(buttonContex);
                if (dynamicActionInfo == null) {
                    dynamicActionInfo = new DynamicActionInfo(buttonContex);
                    _dynamicActionInfoList.Add(dynamicActionInfo);
                } else if (!dynamicActionInfo.CardInfoHasChanged(cardInfo)) {
                    return;
                }

                dynamicActionInfo.UpdateFromCardInfo(cardInfo);

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

        private void ButtonTextChanged(ButtonTextChangedEvent e) {
            IEnumerable<DynamicActionInfo> dynamicActionsToChange = new List<DynamicActionInfo>();
            lock (_cacheLock) {
                dynamicActionsToChange = _dynamicActionInfoList.FindAllWithContext(e);
            }

            //don't raise events within a lock
            foreach (var dynamicActionInfo in dynamicActionsToChange) {
                dynamicActionInfo.Text = e.Text;
                _eventBus.PublishDynamicActionInfoChanged(dynamicActionInfo);
            }
        }

        private void ButtonToggled(ButtonToggledEvent e) {
            IEnumerable<DynamicActionInfo> dynamicActionsToChange = new List<DynamicActionInfo>();
            lock (_cacheLock) {
                dynamicActionsToChange = _dynamicActionInfoList.FindAllWithContext(e);
            }

            //don't raise events within a lock
            foreach (var dynamicActionInfo in dynamicActionsToChange) {
                dynamicActionInfo.IsToggled = e.IsToggled;
                _eventBus.PublishDynamicActionInfoChanged(dynamicActionInfo);
            }
        }
    }
}
