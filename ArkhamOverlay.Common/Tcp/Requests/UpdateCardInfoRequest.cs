using ArkhamOverlay.Common.Enums;

namespace ArkhamOverlay.Common.Tcp.Requests {
    public class UpdateCardInfoRequest : Request, ICardInfo, IButtonContext {
        public UpdateCardInfoRequest() : base(AoTcpRequest.UpdateCardInfo) {
        }

        public CardGroup CardGroup { get; set; }
        public int CardZoneIndex { get; set; }
        public int Index { get; set; }
        public CardButtonType CardButtonType { get; set; }
        public string Name { get; set; }
        public bool IsToggled { get; set; }
        public bool ImageAvailable { get; set; }
    }
}
