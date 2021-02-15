using ArkhamOverlay.Common.Enums;

namespace ArkhamOverlay.Common.Tcp.Requests {
    public enum ButtonClick { Left, Right }

    public class ClickCardButtonRequest : Request, IButtonContext {
        public ClickCardButtonRequest() : base(AoTcpRequest.ClickCardButton) {
        }

        public CardGroupId CardGroupId { get; set; }
        public int Index { get; set; }
        public ButtonMode ButtonMode { get; set; }
        public ButtonClick Click { get; set; }
    }
}
