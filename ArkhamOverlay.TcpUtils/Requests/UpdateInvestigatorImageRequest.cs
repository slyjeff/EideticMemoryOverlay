namespace ArkhamOverlay.TcpUtils.Requests {
    public class UpdateInvestigatorImageRequest : Request {
        public UpdateInvestigatorImageRequest() : base(AoTcpRequest.UpdateInvestigatorImage) {
        }

        public Deck Deck { get; set; }
        public byte[] Bytes { get; set; }
    }
}
