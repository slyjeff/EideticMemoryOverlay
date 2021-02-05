using ArkhamOverlay.TcpUtils;
using StreamDeckPlugin.Events;
using StreamDeckPlugin.Services;
using System.Collections.Generic;
using System.Linq;

namespace StreamDeckPlugin.Utils {
    public enum DynamicActionMode { Pool, Set }

    public interface IDynamicActionInfoStore {
        void UpdateDynamicActionInfo(Deck deck, int index, DynamicActionMode mode, ICardInfo cardInfo);
        IDynamicActionInfo GetDynamicActionInfo(Deck deck, int cardButtonIndex, DynamicActionMode mode);
    }

    public class DynamicActionInfoStore : IDynamicActionInfoStore {
        private readonly object _cacheLock = new object();
        private readonly IList<DynamicActionInfo> _dynamicActionInfoList = new List<DynamicActionInfo>();

        private readonly IEventBus _eventBus;

        public DynamicActionInfoStore(IEventBus eventBus) {
            _eventBus = eventBus;
        }

        public IDynamicActionInfo GetDynamicActionInfo(Deck deck, int index, DynamicActionMode mode) {
            lock (_cacheLock) {
                return _dynamicActionInfoList.FirstOrDefault(x => x.Deck == deck && x.Index == index && x.Mode == mode);
            }
        }

        public void UpdateDynamicActionInfo(Deck deck, int index, DynamicActionMode mode, ICardInfo cardInfo) {
            lock (_cacheLock) {
                var dynamicActionInfo = _dynamicActionInfoList.FirstOrDefault(x => x.Deck == deck && x.Index == index && x.Mode == mode);
                if (dynamicActionInfo == null) {
                    dynamicActionInfo = new DynamicActionInfo(deck, index, mode);
                    _dynamicActionInfoList.Add(dynamicActionInfo);
                } else if (!dynamicActionInfo.CardInfoHasChanged(cardInfo)) {
                    return;
                }

                dynamicActionInfo.UpdateFromCardInfo(cardInfo);

                _eventBus.DynamicActionInfoChanged(dynamicActionInfo);
            }
        }
    }
}
