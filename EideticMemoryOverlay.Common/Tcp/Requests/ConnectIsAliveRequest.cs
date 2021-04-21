namespace Emo.Common.Tcp.Requests {
    public class ConnectIsAliveRequest : Request {
        public ConnectIsAliveRequest() : base(AoTcpRequest.ConnectionIsAlive) {
        }
    }
}
