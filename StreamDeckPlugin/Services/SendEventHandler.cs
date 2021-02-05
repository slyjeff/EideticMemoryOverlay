using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
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

        public SendEventHandler(IEventBus eventBus, IDynamicActionInfoStore dynamicActionInfoStore, IImageService imageService) {
            _eventBus = eventBus;
            _dynamicActionInfoStore = dynamicActionInfoStore;
            _imageService = imageService;

            eventBus.SubscribeToRegisterForUpdatesRequest(RegisterForUpdates);
            eventBus.SubscribeToClearAllCardsRequest(ClearAllCards);
            eventBus.SubscribeToGetButtonInfoRequest(GetCardInfo);
            eventBus.SubscribeToDynamicButtonClickRequest(DynamicButtonClicked);
            eventBus.SubscribeToGetInvestigatorImageRequest(GetInvestigatorImage);
            eventBus.SubscribeToShowDeckListRequest(ShowDeckList);
            eventBus.SubscribeToTakeSnapshotRequest(TakeSnapshot);
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
        public void RegisterForUpdates(Events.RegisterForUpdatesRequest registerForUpdatesRequest) {
            var request = new ArkhamOverlay.TcpUtils.Requests.RegisterForUpdatesRequest { Port = StreamDeckTcpInfo.Port };
            SendRequest<OkResponse>(request);
        }

        public void ClearAllCards(Events.ClearAllCardsRequest clearAllCardsRequest) {
            SendRequest(new ArkhamOverlay.TcpUtils.Requests.ClearAllCardsRequest());
        }

        private void GetCardInfo(GetButtonInfoRequest getButtonInfoRequest) {
            var request = new GetCardInfoRequest {
                Deck = getButtonInfoRequest.Deck,
                Index = getButtonInfoRequest.Index,
                FromCardSet = getButtonInfoRequest.Mode == DynamicActionMode.Set,
            };

            var response = SendRequest<CardInfoResponse>(request);
            if (response != null) {
                _dynamicActionInfoStore.UpdateDynamicActionInfo(getButtonInfoRequest.Deck, getButtonInfoRequest.Index, getButtonInfoRequest.Mode, response);
            }
        }

        private void GetButtonImage(GetButtonImageRequest getButtonImageRequest) {
            var request = new ButtonImageRequest {
                Deck = getButtonImageRequest.Deck,
                Index = getButtonImageRequest.Index,
                FromCardSet = getButtonImageRequest.Mode == DynamicActionMode.Set,
            };

            var response = SendRequest<ButtonImageResponse>(request);
            if (response != null) {
                _imageService.UpdateButtonImage(response.Name, response.Bytes);
            }
        }

        private void DynamicButtonClicked(DynamicButtonClickRequest dynamicButtonClickRequest) {
            var request = new ClickCardButtonRequest {
                Deck = dynamicButtonClickRequest.Deck, 
                Index = dynamicButtonClickRequest.Index, 
                FromCardSet = dynamicButtonClickRequest.Mode == DynamicActionMode.Set, 
                Click = dynamicButtonClickRequest.IsLeftClick ? ButtonClick.Left : ButtonClick.Right
            };
            SendRequest(request);
        }

        private void GetInvestigatorImage(Events.GetInvestigatorImageRequest getInvestigatorImageRequest) {
            SendRequest(new ArkhamOverlay.TcpUtils.Requests.GetInvestigatorImageRequest { Deck = getInvestigatorImageRequest.Deck });
        }

        private void ShowDeckList(Events.ShowDeckListRequest showDeckListRequest) {
            SendRequest(new ArkhamOverlay.TcpUtils.Requests.ShowDeckListRequest { Deck = showDeckListRequest.Deck });
        }

        private void TakeSnapshot(TakeSnapshotRequest takeSnapshotRequest) {
            SendRequest(new SnapshotRequest());
        }

        private void GetStatValue(GetStatValueRequest getStatValueRequest) {
            var response = SendRequest<StatValueResponse>(new StatValueRequest { Deck = getStatValueRequest.Deck, StatType = getStatValueRequest.StatType });
            if (response == null) {
                return;
            }

            _eventBus.PublishStatUpdatedEvent(getStatValueRequest.Deck, getStatValueRequest.StatType, response.Value);
        }

        private void ChangeStatValue(Events.ChangeStatValueRequest changeStatValueRequest) {
            var response = SendRequest<StatValueResponse>(new ArkhamOverlay.TcpUtils.Requests.ChangeStatValueRequest { Deck = changeStatValueRequest.Deck, StatType = changeStatValueRequest.StatType, Increase = changeStatValueRequest.Increase });
            if (response == null) {
                return;
            }

            _eventBus.PublishStatUpdatedEvent(changeStatValueRequest.Deck, changeStatValueRequest.StatType, response.Value);
        }

        #endregion
    }
}
