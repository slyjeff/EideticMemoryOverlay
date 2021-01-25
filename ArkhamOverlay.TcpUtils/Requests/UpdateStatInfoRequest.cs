namespace ArkhamOverlay.TcpUtils.Requests {
    public class UpdateStatInfoRequest : Request {
        public UpdateStatInfoRequest() : base(AoTcpRequest.UpdateStatInfo) {
        }

        public Deck Deck { get; set; }
        public int Value { get; set; }
        public StatType StatType { get; set; }
    }
}
