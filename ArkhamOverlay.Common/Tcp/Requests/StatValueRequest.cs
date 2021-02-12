using ArkhamOverlay.Common.Enums;

namespace ArkhamOverlay.Common.Tcp.Requests {
    public class StatValueRequest : Request {
        public StatValueRequest() : base(AoTcpRequest.StatValue) {
        }

        public CardGroup Deck { get; set; }
        public StatType StatType { get; set; }
    }
}
