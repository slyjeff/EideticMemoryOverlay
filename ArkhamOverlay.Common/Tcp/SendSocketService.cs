using ArkhamOverlay.Common.Tcp.Requests;
using ArkhamOverlay.Common.Tcp.Responses;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ArkhamOverlay.Common.Tcp {
    public static class SendSocketService {
        public static string SendRequest(Request request, int port) {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            var remoteEP = new IPEndPoint(ipAddress, port);

            var sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try {
                sender.Connect(remoteEP);
                try {
                    var payload = Encoding.ASCII.GetBytes(request.ToString());

                    int bytesSent = sender.Send(payload);

                    var responseData = string.Empty;

                    do {
                        var bytes = new byte[1014];
                        int bytesRec = sender.Receive(bytes);
                        responseData += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    } while (responseData.IndexOf("<EOF>") == -1);


                    return responseData.Substring(0, responseData.IndexOf("<EOF>"));
                } finally {
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
            } catch {
                //errorr connecting- will happen a lot if the app isn't there
                return null;
            }
        }

        public static T SendRequest<T>(Request request, int port) where T : Response {
            var result = SendRequest(request, port);
            if (result == null) {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}
