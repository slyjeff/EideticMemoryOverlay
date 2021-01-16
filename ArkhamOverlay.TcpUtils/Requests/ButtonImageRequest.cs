namespace ArkhamOverlay.TcpUtils.Requests {
    public class ButtonImageRequest : Request {
        public ButtonImageRequest() : base(AoTcpRequest.GetButtonImage) {
        }

        public Deck Deck { get; set; }
        public int Index { get; set; }
    }
}
