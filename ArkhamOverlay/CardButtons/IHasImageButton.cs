using System.Windows.Media;

namespace ArkhamOverlay.CardButtons {
    public interface IHasImageButton {
        string Name { get; }
        CardType ImageCardType { get; }
        ImageSource Image { get; set; }
        ImageSource ButtonImage { get; set; }
        byte[] ButtonImageAsBytes { get; set; }
    }
}
