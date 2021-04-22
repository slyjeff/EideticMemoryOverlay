using System;
using System.Windows.Media;

namespace EideticMemoryOverlay.PluginApi {
    /// <summary>
    /// All information about a card, including images from either arkham db or local stored
    /// </summary>
    public abstract class CardInfo : IHasImageButton {
        public CardInfo() {
        }

        public CardInfo(string name, string code, int count, bool cardBack, string imageSrc) {
            Name = cardBack ? name + " (Back)" : name;
            Code = code;
            ImageId = cardBack ? code + " (Back)" : code;
            Count = count;
            ImageSource = imageSrc;
        }

        public string Name { get; }
        public string Code { get; }
        public string ImageId { get; }
        public int Count { get; set; }
        public string ImageSource { get; set; }
        public ImageSource Image { get; set; }
        public ImageSource ButtonImage { get; set; }

        private byte[] _buttonImageAsBytes;
        public byte[] ButtonImageAsBytes {
            get => _buttonImageAsBytes;
            set {
                _buttonImageAsBytes = value;
                ButtonImageLoaded?.Invoke();
            }
        }

        public virtual bool IsHorizontal { get { return false; } }

        public CardInfo FlipSideCard { get; set; }

        public event Action ButtonImageLoaded;
    }
}
