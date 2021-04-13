namespace Emo.Common.Tcp.Requests {
    public class RegisterForUpdatesRequest : Request {
        public RegisterForUpdatesRequest() : base(AoTcpRequest.RegisterForUpdates) {
        }
        public int Port { get; set; }
    }
}
