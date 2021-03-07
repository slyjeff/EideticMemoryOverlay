using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Events;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Tcp;
using ArkhamOverlay.Common.Tcp.Requests;
using ArkhamOverlay.Common.Tcp.Responses;
using Newtonsoft.Json;
using StreamDeckPlugin.Events;
using StreamDeckPlugin.Utils;

namespace StreamDeckPlugin.Services {
    public interface ISendEventHandler {
    }

    public class SendEventHandler : ISendEventHandler {
        private readonly IEventBus _eventBus;
        private readonly ICardGroupStore _cardGroupStore;
        private readonly IDynamicActionInfoStore _dynamicActionInfoStore;
        private readonly IImageService _imageService;

        public SendEventHandler(IEventBus eventBus, ICrossAppEventBus crossAppEventBus, ICardGroupStore cardGroupStore, IDynamicActionInfoStore dynamicActionInfoStore, IImageService imageService) {
            _eventBus = eventBus;
            _cardGroupStore = cardGroupStore;
            _dynamicActionInfoStore = dynamicActionInfoStore;
            _imageService = imageService;

            crossAppEventBus.SendMessage += (request) => {
                SendRequest(request);
            };

            eventBus.SubscribeToEstablishConnectionToUiRequest(RegisterForUpdates);
            eventBus.SubscribeToGetButtonInfoRequest(GetCardInfo);
            eventBus.SubscribeToGetStatValueRequest(GetStatValue);
            eventBus.SubscribeToStatValueRequest(ChangeStatValue);
            eventBus.SubscribeToGetButtonImageRequest(GetButtonImageRequestHandler);
        }

        private string SendRequest(Request request) {
            return SendSocketService.SendRequest(request, TcpInfo.ArkhamOverlayPort);
        }

        private T SendRequest<T>(Request request) where T : Response {
            var response = SendRequest(request);
            if (response == null) {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(response);
        }

        #region Event Handlers
        public void RegisterForUpdates(EstablishConnectionToUiRequest registerForUpdatesRequest) {
            var request = new RegisterForUpdatesRequest { Port = StreamDeckTcpInfo.Port };
            var response = SendRequest<RegisterForUpdatesResponse>(request);
            if (response != null) {
                foreach (var cardGroupInfo in response.CardGroupInfo) {
                    _cardGroupStore.UpdateCardGroupInfo(cardGroupInfo);
                }

                foreach (var button in response.Buttons) {
                    _dynamicActionInfoStore.UpdateDynamicActionInfo(button, button);
                }
            }
        }

        private void GetCardInfo(GetButtonInfoRequest getButtonInfoRequest) {
            var request = new GetCardInfoRequest {
                CardGroupId = getButtonInfoRequest.CardGroupId,
                ButtonMode = getButtonInfoRequest.ButtonMode,
                ZoneIndex = getButtonInfoRequest.ZoneIndex,
                Index = getButtonInfoRequest.Index,
            };

            var response = SendRequest<CardInfoResponse>(request);
            if (response != null) {
                _dynamicActionInfoStore.UpdateDynamicActionInfo(getButtonInfoRequest, response);
            }
        }

        private void GetButtonImageRequestHandler(GetButtonImageRequest getButtonImageRequest) {
            var request = new ButtonImageRequest {
                CardGroupId = getButtonImageRequest.CardGroupId,
                ButtonMode = getButtonImageRequest.ButtonMode,
                ZoneIndex = getButtonImageRequest.ZoneIndex,
                Index = getButtonImageRequest.Index,
            };

            var response = SendRequest<ButtonImageResponse>(request);
            if (response != null) {
                _imageService.UpdateButtonImage(response.Code, response.Bytes);
            }
        }

        private void GetStatValue(GetStatValueRequest getStatValueRequest) {
            var response = SendRequest<StatValueResponse>(new StatValueRequest { Deck = getStatValueRequest.Deck, StatType = getStatValueRequest.StatType });
            if (response == null) {
                return;
            }

            _eventBus.PublishStatUpdated(getStatValueRequest.Deck, getStatValueRequest.StatType, response.Value);
        }

        private void ChangeStatValue(Events.ChangeStatValueRequest changeStatValueRequest) {
            var response = SendRequest<StatValueResponse>(new ArkhamOverlay.Common.Tcp.Requests.ChangeStatValueRequest { Deck = changeStatValueRequest.Deck, StatType = changeStatValueRequest.StatType, Increase = changeStatValueRequest.Increase });
            if (response == null) {
                return;
            }

            _eventBus.PublishStatUpdated(changeStatValueRequest.Deck, changeStatValueRequest.StatType, response.Value);
        }

        #endregion
    }
}
