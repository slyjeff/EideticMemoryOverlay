using ArkhamOverlay.Common.Enums;

namespace ArkhamOverlay.Common.Tcp.Requests {
    public class ChangeStatValueRequest : Request {
        public ChangeStatValueRequest() : base(AoTcpRequest.ChangeStatValue) {
        }

        public Deck Deck { get; set; }
        public StatType StatType { get; set; }
        public bool Increase { get; set; }
    }
}
