using ArkhamOverlay.Common.Enums;

namespace ArkhamOverlay.Common.Tcp.Requests {
    public class GetInvestigatorImageRequest : Request {
        public GetInvestigatorImageRequest() : base(AoTcpRequest.GetInvestigatorImage) {
        }

        public CardGroup CardGroup { get; set; }
    }
}
