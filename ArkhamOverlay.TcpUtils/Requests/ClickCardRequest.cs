namespace ArkhamOverlay.TcpUtils.Requests {
    public class ClickCardButtonRequest : Request {
        public ClickCardButtonRequest() : base(AoTcpRequest.ClickCardButton) {
        }

        public string Deck { get; set; }
        public int Index { get; set; }
    }
}
