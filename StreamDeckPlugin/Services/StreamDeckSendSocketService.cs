using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using Newtonsoft.Json;
using StreamDeckPlugin.Events;
using StreamDeckPlugin.Utils;

namespace StreamDeckPlugin.Services {
    public interface ISendSocketService {
        string SendRequest(Request request);
        T SendRequest<T>(Request request) where T : Response;
    }

    public class StreamDeckSendSocketService : ISendSocketService {
        private readonly IDynamicActionInfoStore _dynamicActionInfoStore;

        public StreamDeckSendSocketService(IEventBus eventBus, IDynamicActionInfoStore dynamicActionInfoStore) {
            _dynamicActionInfoStore = dynamicActionInfoStore;

            eventBus.OnClearAllCards(ClearAllCards);
            eventBus.OnGetButtonInfo(GetCardInfo);
            eventBus.OnDynamicButtonClicked(DynamicButtonClicked);
            eventBus.OnGetInvestigatorImage(GetInvestigatorImage);
            eventBus.OnShowDeckList(ShowDeckList);
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

        private void GetCardInfo(Deck deck, int index, DynamicActionMode mode) {
            var request = new GetCardInfoRequest {
                Deck = deck,
                Index = index,
                FromCardSet = mode == DynamicActionMode.Set,
            };

            var response = SendRequest<CardInfoResponse>(request);
            if (response != null) {
                _dynamicActionInfoStore.UpdateDynamicActionInfo(deck, index, mode, response);
            }
        }

        private void DynamicButtonClicked(Deck deck, int index, DynamicActionMode mode, bool isLeftClick) {
            var request = new ClickCardButtonRequest {
                Deck = deck, 
                Index = index, 
                FromCardSet = mode == DynamicActionMode.Set, 
                Click = isLeftClick ? ButtonClick.Left : ButtonClick.Right
            };
            SendRequest(request);
        }

        private void GetInvestigatorImage(Deck deck) {
            SendRequest(new GetInvestigatorImageRequest { Deck = deck });
        }

        private void ShowDeckList(Deck deck) {
            SendRequest(new ShowDeckListRequest { Deck = deck });
        }

        #endregion
    }
}
