using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Utils;

namespace ArkhamOverlay.Common.Tcp.Requests {
    public class GetCardInfoRequest : Request, IButtonContext {
        public GetCardInfoRequest() : base(AoTcpRequest.GetButtonInfo) {
        }

        public CardGroupId CardGroupId { get; set; }
        public ButtonMode ButtonMode { get; set; }
        public int ZoneIndex { get; set; }
        public int Index { get; set; }
    }
}
