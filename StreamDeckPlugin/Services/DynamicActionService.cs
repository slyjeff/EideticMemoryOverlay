using ArkhamOverlay.TcpUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamDeckPlugin.Services {
    public enum DynamicActionMode { Pool, Set }

    public interface IDynamicActionService {
        void UpdateDynamicAction(Deck deck, int index, DynamicActionMode mode, ICardInfo cardInfo);
        IDynamicAction GetDynamicAction(Deck deck, int cardButtonIndex, DynamicActionMode mode);
        event Action<IDynamicAction> DynamicActionChanged;
    }

    public interface IDynamicAction {
        Deck Deck { get; }
        int Index { get; }
        DynamicActionMode Mode { get; }
        string ImageId { get; set; }
        string Text { get; set; }
        bool IsImageAvailable { get; set; }
        bool IsToggled { get; set; }
    }

    public class DynamicActionService : IDynamicActionService {
        private readonly object _cacheLock = new object();
        private readonly IList<DynamicAction> _dynamicActions = new List<DynamicAction>();

        public event Action<IDynamicAction> DynamicActionChanged;

        public IDynamicAction GetDynamicAction(Deck deck, int index, DynamicActionMode mode) {
            lock (_cacheLock) {
                return _dynamicActions.FirstOrDefault(x => x.Deck == deck && x.Index == index && x.Mode == mode);
            }
        }

        public void UpdateDynamicAction(Deck deck, int index, DynamicActionMode mode, ICardInfo cardInfo) {
            lock (_cacheLock) {
                var dynamicAction = _dynamicActions.FirstOrDefault(x => x.Deck == deck && x.Index == index && x.Mode == mode);
                if (dynamicAction == null) {
                    dynamicAction = new DynamicAction(deck, index, mode);
                    _dynamicActions.Add(dynamicAction);
                }

                dynamicAction.ImageId = cardInfo.Name;
                dynamicAction.Text = cardInfo.Name;
                dynamicAction.IsImageAvailable = cardInfo.ImageAvailable;
                dynamicAction.IsToggled = cardInfo.IsToggled;

                if (dynamicAction.IsChanged) {
                    dynamicAction.IsChanged = false;
                    DynamicActionChanged?.Invoke(dynamicAction);
                }
            }
        }
    }

    public class DynamicAction : IDynamicAction {
        public DynamicAction(Deck deck, int index, DynamicActionMode mode) {
            Deck = deck;
            Index = index;
            Mode = mode;
        }

        public Deck Deck { get; }
        public int Index{ get; }
        public DynamicActionMode Mode { get; }

        public bool IsChanged = true;

        private string _imageId;
        public string ImageId {
            get => _imageId;
            set {
                if (_imageId == value) {
                    return;
                }

                _imageId = value;
                IsChanged = true;
            }
        }

        private string _text;
        public string Text {
            get => _text;
            set {
                if (_text == value) {
                    return;
                }

                _text = value;
                IsChanged = true;
            }
        }

        private bool _isImageAvailable;
        public bool IsImageAvailable {
            get => _isImageAvailable;
            set {
                if (_isImageAvailable == value) {
                    return;
                }

                _isImageAvailable = value;
                IsChanged = true;
            }
        }

        private bool _isToggled;
        public bool IsToggled {
            get => _isToggled;
            set {
                if (_isToggled == value) {
                    return;
                }

                _isToggled = value;
                IsChanged = true;
            }
        }
    }
}
