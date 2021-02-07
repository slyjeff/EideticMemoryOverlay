using ArkhamOverlay.Common.Enums;

namespace ArkhamOverlay.Common.Tcp.Requests {
    public class ShowDeckListRequest : Request {
        public ShowDeckListRequest() : base(AoTcpRequest.ShowDeckList) {
        }

        public Deck Deck { get; set; }
    }
}
