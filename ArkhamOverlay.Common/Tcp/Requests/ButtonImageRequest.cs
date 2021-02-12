using ArkhamOverlay.Common.Enums;

namespace ArkhamOverlay.Common.Tcp.Requests {
    public class ButtonImageRequest : Request {
        public ButtonImageRequest() : base(AoTcpRequest.GetButtonImage) {
        }

        public CardGroup CardGroup { get; set; }
        public int Index { get; set; }
        public bool FromCardSet { get; set; }
    }
}
