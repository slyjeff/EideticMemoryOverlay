using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Utils;
using System.Collections.Generic;

namespace ArkhamOverlay.Common.Tcp.Responses {
    public class CardInfoResponse : Response, ICardInfo {
        public CardButtonType CardButtonType { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsToggled { get; set; }
        public bool ImageAvailable { get; set; }
        public IList<ButtonOption> ButtonOptions { get; set; }
    }
}
