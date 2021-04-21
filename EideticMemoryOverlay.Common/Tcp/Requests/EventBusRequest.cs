namespace Emo.Common.Tcp.Requests {
    public class EventBusRequest : Request {
        public EventBusRequest() : base(AoTcpRequest.EventBus) {
        }

        public string EventClassName { get; set; }
        public string SerializedEventData { get; set; }
    }
}
