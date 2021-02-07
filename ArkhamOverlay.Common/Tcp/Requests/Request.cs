using Newtonsoft.Json;

namespace ArkhamOverlay.Common.Tcp.Requests {
    public abstract class Request {
        private readonly AoTcpRequest _aoTcpRequest;

        public Request(AoTcpRequest aoTcpRequest) {
            _aoTcpRequest = aoTcpRequest;
        }

        public override string ToString() {
            return _aoTcpRequest.ToString() + ":" + JsonConvert.SerializeObject(this) + "<EOF>";
        }
    }
}
