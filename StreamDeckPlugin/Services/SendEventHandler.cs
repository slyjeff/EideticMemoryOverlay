﻿using ArkhamOverlay.Common.Events;
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
        private readonly IDynamicActionInfoStore _dynamicActionInfoStore;
        private readonly IImageService _imageService;

        public SendEventHandler(IEventBus eventBus, ICrossAppEventBus crossAppEventBus, IDynamicActionInfoStore dynamicActionInfoStore, IImageService imageService) {
            _eventBus = eventBus;
            _dynamicActionInfoStore = dynamicActionInfoStore;
            _imageService = imageService;

            crossAppEventBus.SendMessage += (request) => {
                SendRequest(request);
            };

            eventBus.SubscribeToEstablishConnectionToUiRequest(RegisterForUpdates);
            eventBus.SubscribeToClearAllCardsRequest(ClearAllCards);
            eventBus.SubscribeToGetButtonInfoRequest(GetCardInfo);
            eventBus.SubscribeToDynamicButtonClickRequest(DynamicButtonClicked);
            eventBus.SubscribeToGetInvestigatorImageRequest(GetInvestigatorImage);
            eventBus.SubscribeToGetStatValueRequest(GetStatValue);
            eventBus.SubscribeToStatValueRequest(ChangeStatValue);
            eventBus.SubscribeToGetButtonImageRequest(GetButtonImage);
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
            SendRequest<OkResponse>(request);
        }

        public void ClearAllCards(Events.ClearAllCardsRequest clearAllCardsRequest) {
            SendRequest(new ArkhamOverlay.Common.Tcp.Requests.ClearAllCardsRequest());
        }

        private void GetCardInfo(GetButtonInfoRequest getButtonInfoRequest) {
            var request = new GetCardInfoRequest {
                GardGroup = getButtonInfoRequest.CardGroup,
                Index = getButtonInfoRequest.Index,
                FromCardSet = getButtonInfoRequest.CardZoneIndex == 1,
            };

            var response = SendRequest<CardInfoResponse>(request);
            if (response != null) {
                _dynamicActionInfoStore.UpdateDynamicActionInfo(getButtonInfoRequest, response);
            }
        }

        private void GetButtonImage(GetButtonImageRequest getButtonImageRequest) {
            var request = new ButtonImageRequest {
                CardGroup = getButtonImageRequest.CardGroup,
                Index = getButtonImageRequest.Index,
                FromCardSet = getButtonImageRequest.CardZoneIndex == 1,
            };

            var response = SendRequest<ButtonImageResponse>(request);
            if (response != null) {
                _imageService.UpdateButtonImage(response.Name, response.Bytes);
            }
        }

        private void DynamicButtonClicked(DynamicButtonClickRequest dynamicButtonClickRequest) {
            var request = new ClickCardButtonRequest {
                CardGroup = dynamicButtonClickRequest.CardGroup, 
                Index = dynamicButtonClickRequest.Index, 
                FromCardSet = dynamicButtonClickRequest.CardZoneIndex == 1, 
                Click = dynamicButtonClickRequest.IsLeftClick ? ButtonClick.Left : ButtonClick.Right
            };
            SendRequest(request);
        }

        private void GetInvestigatorImage(Events.GetInvestigatorImageRequest getInvestigatorImageRequest) {
            SendRequest(new ArkhamOverlay.Common.Tcp.Requests.GetInvestigatorImageRequest { CardGroup = getInvestigatorImageRequest.CardGroup });
        }

        private void GetStatValue(GetStatValueRequest getStatValueRequest) {
            var response = SendRequest<StatValueResponse>(new StatValueRequest { Deck = getStatValueRequest.Deck, StatType = getStatValueRequest.StatType });
            if (response == null) {
                return;
            }

            _eventBus.PublishStatUpdatedEvent(getStatValueRequest.Deck, getStatValueRequest.StatType, response.Value);
        }

        private void ChangeStatValue(Events.ChangeStatValueRequest changeStatValueRequest) {
            var response = SendRequest<StatValueResponse>(new ArkhamOverlay.Common.Tcp.Requests.ChangeStatValueRequest { Deck = changeStatValueRequest.Deck, StatType = changeStatValueRequest.StatType, Increase = changeStatValueRequest.Increase });
            if (response == null) {
                return;
            }

            _eventBus.PublishStatUpdatedEvent(changeStatValueRequest.Deck, changeStatValueRequest.StatType, response.Value);
        }

        #endregion
    }
}
