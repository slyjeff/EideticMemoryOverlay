using Newtonsoft.Json;

namespace ArkhamOverlay.TcpUtils.Requests {
    public abstract class Request {
        private readonly AoTcpRequest _aoTcpRequest;

        public Request(AoTcpRequest aoTcpRequest) {
            _aoTcpRequest = aoTcpRequest;
        }

        public override string ToString() {
            return _aoTcpRequest.AsString() + ":" + JsonConvert.SerializeObject(this) + "<EOF>";
        }
    }
}
