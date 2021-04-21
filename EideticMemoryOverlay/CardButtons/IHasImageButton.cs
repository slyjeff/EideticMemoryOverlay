using System;
using System.Windows.Media;

namespace Emo.CardButtons {
    public interface IHasImageButton {
        string Name { get; }
        string ImageId { get; }
        CardType ImageCardType { get; }
        string ImageSource { get; set; }
        ImageSource Image { get; set; }
        ImageSource ButtonImage { get; set; }
        byte[] ButtonImageAsBytes { get; set; }
    }
}
