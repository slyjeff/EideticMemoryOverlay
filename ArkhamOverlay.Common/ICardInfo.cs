using ArkhamOverlay.Common.Enums;

namespace ArkhamOverlay.Common {
    public interface ICardInfo {
        CardButtonType CardButtonType { get; set; }
        string Name { get; set; }
        bool IsToggled { get; set; }
        bool ImageAvailable { get; set; }
    }
}
