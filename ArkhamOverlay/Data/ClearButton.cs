using System.Windows.Media;

namespace ArkhamOverlay.Data {
    public class ClearButton : ICardButton {
        public Brush Background {
            get {
                return new SolidColorBrush(Colors.Black);
            }
        }
    }

}
