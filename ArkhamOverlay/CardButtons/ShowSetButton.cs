using ArkhamOverlay.Utils;
using System.Windows.Media;

namespace ArkhamOverlay.CardButtons {
    public class ShowSetButton : CardButton {
        public ShowSetButton() {
            Name = "";
        }

        public override void LeftClick() {
            if (SelectableCards == null) {
                return;
            }

            IsVisible = !IsVisible;
            SelectableCards.OnSetUpdated();
        }

        public void SetHidden() {
            IsVisible = false;
        }

        public ImageSource ButtonImage { get { return ImageUtils.CreateSolidColorImage(Colors.DarkGray); } }

        private bool _isVisible;
        public bool IsVisible {
            get => _isVisible;
            private set {
                _isVisible = value;
                NotifyPropertyChanged(nameof(BorderBrush));
            }
        }

        public override Brush BorderBrush { get { return IsVisible ? new SolidColorBrush(Colors.DarkGoldenrod) : new SolidColorBrush(Colors.Black); } }
    }
}
