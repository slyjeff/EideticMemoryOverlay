namespace ArkhamOverlay.Common.Tcp.Responses {
    public class OkResponse : Response {
        public OkResponse() {
            Ok = true;
        }

        public bool Ok { get; set; }
    }
}
