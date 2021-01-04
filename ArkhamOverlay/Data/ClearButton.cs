using ArkhamOverlay.Utils;
using System.Windows.Media;

namespace ArkhamOverlay.Data {
    public class ClearButton : ICardButton {
        public string Name {
            get {
                return "Clear Cards";
            }
        }

        public Brush Background {
            get {
                return new SolidColorBrush(Colors.Black);
            }
        }

        public Brush BorderBrush {
            get {
                return Background;
            }
        }

        public SelectableCards SelectableCards { get; internal set; }

        public void Click() {
            if (SelectableCards == null) {
                return;
            }

            SelectableCards.ClearSelections();
        }

        public ImageSource ButtonImage { get { return ImageUtils.CreateSolidColorImage(Colors.DarkGray); } }
    }
}
