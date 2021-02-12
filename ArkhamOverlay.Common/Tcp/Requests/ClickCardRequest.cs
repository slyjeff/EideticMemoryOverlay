using ArkhamOverlay.Common.Enums;

namespace ArkhamOverlay.Common.Tcp.Requests {
    public enum ButtonClick { Left, Right }

    public class ClickCardButtonRequest : Request {
        public ClickCardButtonRequest() : base(AoTcpRequest.ClickCardButton) {
        }

        public CardGroup CardGroup { get; set; }
        public int Index { get; set; }
        public ButtonClick Click { get; set; }
        public bool FromCardSet { get; set; }
    }
}
