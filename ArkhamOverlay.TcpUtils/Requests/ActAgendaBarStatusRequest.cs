namespace ArkhamOverlay.TcpUtils.Requests {
    public class ActAgendaBarStatusRequest : Request {
        public ActAgendaBarStatusRequest() : base(AoTcpRequest.ActAgendaBarStatusRequest) {
        }

        public bool IsVisible { get; set; }
    }
}