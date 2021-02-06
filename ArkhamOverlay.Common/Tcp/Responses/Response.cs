using Newtonsoft.Json;

namespace ArkhamOverlay.Common.Tcp.Responses {
    public abstract class Response {
        public override string ToString() {
            return JsonConvert.SerializeObject(this) + "<EOF>";
        }
    }
}
