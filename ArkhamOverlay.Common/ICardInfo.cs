using ArkhamOverlay.Common.Utils;
using System.Collections.Generic;

namespace ArkhamOverlay.Common {
    public interface ICardInfo {
        string Name { get; }
        string Code { get; }
        bool IsToggled { get; }
        bool ImageAvailable { get; }
        IList<ButtonOption> ButtonOptions { get; }
    }
}
