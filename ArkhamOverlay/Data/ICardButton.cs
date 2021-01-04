using System.Windows.Media;

namespace ArkhamOverlay.Data {
    public interface ICardButton {
        string Name { get; }

        ImageSource ButtonImage { get; }

        void Click();
    }
}
