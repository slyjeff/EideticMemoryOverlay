namespace ArkhamOverlay.TcpUtils.Requests {
    public class GetInvestigatorImageRequest : Request {
        public GetInvestigatorImageRequest() : base(AoTcpRequest.GetInvestigatorImage) {
        }

        public Deck Deck { get; set; }
    }
}
