namespace ArkhamOverlay.TcpUtils.Requests {
    public class ShowDeckListRequest : Request {
        public ShowDeckListRequest() : base(AoTcpRequest.ShowDeckList) {
        }

        public Deck Deck { get; set; }
    }
}
