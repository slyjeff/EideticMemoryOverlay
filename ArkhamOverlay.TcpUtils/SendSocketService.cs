using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ArkhamOverlay.TcpUtils {
    public static class SendSocketService {
        public static string SendRequest(Request request) {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            var remoteEP = new IPEndPoint(ipAddress, TcpInfo.Port);

            var sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            sender.Connect(remoteEP);
            try {

                var payload = Encoding.ASCII.GetBytes(request.ToString());

                int bytesSent = sender.Send(payload);

                var bytes = new byte[1024];
                int bytesRec = sender.Receive(bytes);
                var responseData = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                return responseData.Substring(0, responseData.IndexOf("<EOF>"));
            } finally {
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
        }

        public static T SendRequest<T>(Request request) where T : Response {
            return JsonConvert.DeserializeObject<T>(SendRequest(request));
        }
    }
}
