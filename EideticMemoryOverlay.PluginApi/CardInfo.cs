using System;
using System.Windows;
using System.Windows.Media;

namespace EideticMemoryOverlay.PluginApi {
    public enum CardInfoSort { Alphabetical, Natural }

    /// <summary>
    /// All information about a card, including images from either arkham db or local stored
    /// </summary>
    public abstract class CardInfo : IHasImageButton {
        public CardInfo() {
        }

        public CardInfo(string name, string code, bool isPlayerCard, int count, bool cardBack, string imageSrc) {
            Name = cardBack ? name + " (Back)" : name;
            Code = code;
            ImageId = cardBack ? code + " (Back)" : code;
            IsPlayerCard = isPlayerCard;
            Count = count;
            ImageSource = imageSrc;
        }

        public string Name { get; }
        public string Code { get; }
        public string ImageId { get; }
        public bool IsPlayerCard { get; }

        public int Count { get; set; }
        public string ImageSource { get; set; }
        public ImageSource Image { get; set; }
        public ImageSource ButtonImage { get; set; }

        public virtual CardInfoSort SortBy { get { return CardInfoSort.Alphabetical; } }

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

        public virtual Point GetCropStartingPoint() {
            return new Point(0, 0);
        }

        public virtual string DeckListName { get { return Name; } }

        public virtual Brush DeckListColor { get { return new SolidColorBrush(Colors.Black); } }
    }
}
