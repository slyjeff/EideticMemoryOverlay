using Newtonsoft.Json;

namespace ArkhamOverlay.TcpUtils.Responses {
    public abstract class Response {
        public override string ToString() {
            return JsonConvert.SerializeObject(this) + "<EOF>";
        }
    }
}
