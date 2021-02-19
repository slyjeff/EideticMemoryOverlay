using ArkhamOverlay.Common.Enums;

namespace ArkhamOverlay.Common {
    /// <summary>
    /// Information about a Card Group- used for both messages and events
    /// </summary>
    public interface ICardGroupInfo {
        CardGroupId CardGroupId { get; }
        string Name { get; }
        bool IsImageAvailable { get; }
        string ImageId { get; }
    }
}
