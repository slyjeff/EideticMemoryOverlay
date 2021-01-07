using ArkhamOverlay.Utils;
using System.Windows.Media;

namespace ArkhamOverlay.CardButtons {
    public class ClearButton : CardButton {
        public ClearButton() {
            Name = "Clear Cards";
        }

        public override void LeftClick() {
            if (SelectableCards == null) {
                return;
            }

            SelectableCards.ClearSelections();
        }

        public ImageSource ButtonImage { get { return ImageUtils.CreateSolidColorImage(Colors.DarkGray); } }
    }
}
