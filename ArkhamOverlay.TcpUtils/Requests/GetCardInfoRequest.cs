namespace ArkhamOverlay.TcpUtils.Requests {
    public class GetCardInfoRequest : Request {
        public GetCardInfoRequest() : base(AoTcpRequest.GetCardInfo) {
        }

        public Deck Deck { get; set; }
        public int Index { get; set; }
    }
}
