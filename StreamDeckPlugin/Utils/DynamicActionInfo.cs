using ArkhamOverlay.TcpUtils;

namespace StreamDeckPlugin.Utils {
    public interface IDynamicActionInfo {
        Deck Deck { get; }
        int Index { get; }
        DynamicActionMode Mode { get; }
        string ImageId { get; set; }
        string Text { get; set; }
        bool IsImageAvailable { get; set; }
        bool IsToggled { get; set; }
    }

    public class DynamicActionInfo : IDynamicActionInfo {
        public DynamicActionInfo(Deck deck, int index, DynamicActionMode mode) {
            Deck = deck;
            Index = index;
            Mode = mode;
        }

        public Deck Deck { get; }
        public int Index { get; }
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
