using ArkhamOverlay.TcpUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamDeckPlugin.Utils {
    public enum DynamicActionMode { Pool, Set }

    public interface IDynamicActionInfoStore {
        void UpdateDynamicAction(Deck deck, int index, DynamicActionMode mode, ICardInfo cardInfo);
        IDynamicActionInfo GetDynamicActionInfo(Deck deck, int cardButtonIndex, DynamicActionMode mode);
        event Action<IDynamicActionInfo> DynamicActionChanged;
    }

    public class DynamicActionInfoStore : IDynamicActionInfoStore {
        private readonly object _cacheLock = new object();
        private readonly IList<DynamicActionInfo> _dynamicActionInfoList = new List<DynamicActionInfo>();

        public event Action<IDynamicActionInfo> DynamicActionChanged;

        public IDynamicActionInfo GetDynamicActionInfo(Deck deck, int index, DynamicActionMode mode) {
            lock (_cacheLock) {
                return _dynamicActionInfoList.FirstOrDefault(x => x.Deck == deck && x.Index == index && x.Mode == mode);
            }
        }

        public void UpdateDynamicAction(Deck deck, int index, DynamicActionMode mode, ICardInfo cardInfo) {
            lock (_cacheLock) {
                var dynamicActionInfo = _dynamicActionInfoList.FirstOrDefault(x => x.Deck == deck && x.Index == index && x.Mode == mode);
                if (dynamicActionInfo == null) {
                    dynamicActionInfo = new DynamicActionInfo(deck, index, mode);
                    _dynamicActionInfoList.Add(dynamicActionInfo);
                }

                dynamicActionInfo.ImageId = cardInfo.Name;
                dynamicActionInfo.Text = cardInfo.Name;
                dynamicActionInfo.IsImageAvailable = cardInfo.ImageAvailable;
                dynamicActionInfo.IsToggled = cardInfo.IsToggled;

                if (dynamicActionInfo.IsChanged) {
                    dynamicActionInfo.IsChanged = false;
                    DynamicActionChanged?.Invoke(dynamicActionInfo);
                }
            }
        }
    }
}
