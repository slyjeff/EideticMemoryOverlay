using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using Newtonsoft.Json;
using StreamDeckPlugin.Events;

namespace StreamDeckPlugin.Services {
    public interface ISendSocketService {
        string SendRequest(Request request);
        T SendRequest<T>(Request request) where T : Response;
    }

    public class StreamDeckSendSocketService : ISendSocketService {

        public StreamDeckSendSocketService(IEventBus eventBus) {
            eventBus.OnClearAllCards(ClearAllCards);
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

        #region Event Handlers

        public void ClearAllCards() {
            SendRequest(new ClearAllCardsRequest());
        }

        #endregion
    }
}
