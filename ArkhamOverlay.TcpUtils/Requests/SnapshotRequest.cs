namespace ArkhamOverlay.TcpUtils.Requests {
    public class SnapshotRequest : Request {
        public SnapshotRequest() : base(AoTcpRequest.GetButtonImage) {
        }

        public Deck Deck { get; set; }
        public int Index { get; set; }
        public bool FromCardSet { get; set; }
    }
}
