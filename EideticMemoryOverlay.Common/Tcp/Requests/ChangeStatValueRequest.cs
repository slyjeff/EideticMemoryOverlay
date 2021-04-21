using Emo.Common.Enums;

namespace Emo.Common.Tcp.Requests {
    public class ChangeStatValueRequest : Request {
        public ChangeStatValueRequest() : base(AoTcpRequest.ChangeStatValue) {
        }

        public CardGroupId Deck { get; set; }
        public StatType StatType { get; set; }
        public bool Increase { get; set; }
    }
}
