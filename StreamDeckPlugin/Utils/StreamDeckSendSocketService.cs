using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using Newtonsoft.Json;

namespace StreamDeckPlugin.Utils {
    class StreamDeckSendSocketService {
        public static string SendRequest(Request request) {
            return SendSocketService.SendRequest(request, TcpInfo.ArkhamOverlayPort);
        }

        public static T SendRequest<T>(Request request) where T : Response {
            var response = SendRequest(request);
            if (response == null) {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(response);
        }
    }

}
