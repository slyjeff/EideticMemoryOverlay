namespace ArkhamOverlay.TcpUtils.Requests {
    public class ChangeStatValueRequest : Request {
        public ChangeStatValueRequest() : base(AoTcpRequest.ChangeStatValue) {
        }

        public Deck Deck { get; set; }
        public StatType StatType { get; set; }
        public bool Increase { get; set; }
    }
}
