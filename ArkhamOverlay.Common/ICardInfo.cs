using ArkhamOverlay.Common.Enums;

namespace ArkhamOverlay.Common {
    public interface ICardInfo {
        string Name { get; }
        bool IsToggled { get; }
        bool ImageAvailable { get; }
    }
}
