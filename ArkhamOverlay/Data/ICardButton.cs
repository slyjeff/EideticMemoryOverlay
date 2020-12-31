using System.Windows.Media;

namespace ArkhamOverlay.Data {
    public interface ICardButton {
        string Name { get; }

        Brush Background { get; }

        void Click();
    }
}
