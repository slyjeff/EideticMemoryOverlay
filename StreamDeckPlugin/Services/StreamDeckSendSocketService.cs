﻿using ArkhamOverlay.TcpUtils;
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
        private readonly IEventBus _eventBus;

        public StreamDeckSendSocketService(IEventBus eventBus, IDynamicActionInfoStore dynamicActionInfoStore) {
            _eventBus = eventBus;
            _dynamicActionInfoStore = dynamicActionInfoStore;

            eventBus.OnClearAllCards(ClearAllCards);
            eventBus.OnGetButtonInfo(GetCardInfo);
            eventBus.OnDynamicButtonClicked(DynamicButtonClicked);
            eventBus.OnGetInvestigatorImage(GetInvestigatorImage);
            eventBus.OnShowDeckList(ShowDeckList);
            eventBus.OnTakeSnapshot(TakeSnapshot);
            eventBus.OnGetStatValue(GetStatValue);
            eventBus.OnChangeStatValue(ChangeStatValue);
            eventBus.OnGetButtonImage(GetButtonImage);
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

        private void TakeSnapshot() {
            SendRequest(new SnapshotRequest());
        }

        private void GetStatValue(Deck deck, StatType statType) {
            var response = SendRequest<StatValueResponse>(new StatValueRequest { Deck = deck, StatType = statType });
            if (response == null) {
                return;
            }

            _eventBus.StatUpdated(deck, statType, response.Value);
        }

        private void ChangeStatValue(Deck deck, StatType statType, bool increase) {
            var response = SendRequest<StatValueResponse>(new ChangeStatValueRequest { Deck = deck, StatType = statType, Increase = increase });
            if (response == null) {
                return;
            }

            _eventBus.StatUpdated(deck, statType, response.Value);
        }

        private void GetButtonImage(Deck deck, int index, DynamicActionMode mode) {
            var request = new ButtonImageRequest {
                Deck = deck,
                Index = index,
                FromCardSet = mode == DynamicActionMode.Set,
            };

            var response = SendRequest<ButtonImageResponse>(request);
            if (response != null) {
                _eventBus.ButtonImageReceived(response.Name, response.Bytes);
            }
        }

        #endregion
    }
}
