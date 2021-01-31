namespace ArkhamOverlay.TcpUtils.Requests {
    public class StatValueRequest : Request {
        public StatValueRequest() : base(AoTcpRequest.StatValue) {
        }

        public Deck Deck { get; set; }
        public StatType StatType { get; set; }
    }
}
