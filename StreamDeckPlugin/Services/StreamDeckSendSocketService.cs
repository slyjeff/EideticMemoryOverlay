using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using Newtonsoft.Json;
using System;

namespace StreamDeckPlugin.Services {
    public interface ISendSocketService {
        string SendRequest(Request request);
        T SendRequest<T>(Request request) where T : Response;
    }

    public class StreamDeckSendSocketService : ISendSocketService {
        public static ISendSocketService Service { get; private set; }

        public StreamDeckSendSocketService() {
            if (Service != null) {
                throw new Exception("Only one instance of Service may be created");
            }

            Service = this;
        }

        public string SendRequest(Request request) {
            return SendSocketService.SendRequest(request, TcpInfo.ArkhamOverlayPort);
        }

        public T SendRequest<T>(Request request) where T : Response {
            var response = SendRequest(request);
            if (response == null) {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(response);
        }
    }

}
