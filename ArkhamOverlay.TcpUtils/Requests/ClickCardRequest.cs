namespace ArkhamOverlay.TcpUtils.Requests {
    public enum ButtonClick { Left, Right }

    public class ClickCardButtonRequest : Request {
        public ClickCardButtonRequest() : base(AoTcpRequest.ClickCardButton) {
        }

        public Deck Deck { get; set; }
        public int Index { get; set; }
        public ButtonClick Click { get; set; }
        public bool FromCardSet { get; set; }
    }
}
