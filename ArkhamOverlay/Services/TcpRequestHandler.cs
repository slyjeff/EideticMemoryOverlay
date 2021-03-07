using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Events;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Tcp;
using ArkhamOverlay.Common.Tcp.Requests;
using ArkhamOverlay.Common.Tcp.Responses;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Data;
using ArkhamOverlay.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ArkhamOverlay.Services {
    internal class TcpRequestHandler : IRequestHandler {
        private readonly AppData _appData;
        private readonly LoggingService _logger;
        private readonly IEventBus _eventBus;
        private readonly ICrossAppEventBus _crossAppEventBus;
        private readonly IBroadcastService _broadcastService;

        public TcpRequestHandler(AppData viewModel, LoggingService loggingService, IEventBus eventBus, ICrossAppEventBus crossAppEventBus, IBroadcastService broadcastService) {
            _appData = viewModel;
            _logger = loggingService;
            _eventBus = eventBus;
            _crossAppEventBus = crossAppEventBus;
            _broadcastService = broadcastService;
        }

        public void HandleRequest(TcpRequest request) {
            _logger.LogMessage($"Handling Request: {request.RequestType}");
            switch (request.RequestType) {
                case AoTcpRequest.GetButtonInfo:
                    HandleGetButtonInfo(request);
                    break;
                case AoTcpRequest.GetButtonImage:
                    HandleGetButtonImage(request);
                    break;
                case AoTcpRequest.RegisterForUpdates:
                    HandleRegisterForUpdates(request);
                    break;
                case AoTcpRequest.StatValue:
                    HandleRequestStatValue(request);
                    break;
                case AoTcpRequest.ChangeStatValue:
                    HandleChangeStatValue(request);
                    break;
                case AoTcpRequest.EventBus:
                    HandleEventBusRequest(request);
                    break;
            }
        }

        private void HandleEventBusRequest(TcpRequest request) {
            SendOkResponse(request.Socket);

            _logger.LogMessage("Handling event bus request");
            var eventBusRequest = JsonConvert.DeserializeObject<EventBusRequest>(request.Body);
            _crossAppEventBus.ReceiveMessage(eventBusRequest);
        }

        private void HandleGetButtonInfo(TcpRequest request) {
            _logger.LogMessage("Handling button info request");
            var getCardInfoRequest = JsonConvert.DeserializeObject<GetCardInfoRequest>(request.Body);
            var cardButton = GetCardButton(getCardInfoRequest);
            SendCardInfoResponse(request.Socket, cardButton);
        }

        private void HandleGetButtonImage(TcpRequest request) {
            _logger.LogMessage("Handling button image request");
            var buttonImageRequest = JsonConvert.DeserializeObject<ButtonImageRequest>(request.Body);

            var cardGroup = _appData.Game.GetCardGroup(buttonImageRequest.CardGroupId);
            byte[] imageAsBytes = null;
            var imageId = string.Empty;
            if (!buttonImageRequest.ButtonMode.HasValue || !buttonImageRequest.Index.HasValue) {
                SendButtonImageResponse(request.Socket, cardGroup.Name, cardGroup.ButtonImageAsBytes);
            } else {
                if (cardGroup.GetButton(buttonImageRequest.ButtonMode.Value, buttonImageRequest.ZoneIndex.Value, buttonImageRequest.Index.Value) is CardInfoButton cardButton) {
                    imageId = cardButton.CardInfo.ImageId;
                    imageAsBytes = cardButton.CardInfo.ButtonImageAsBytes;
                }
            }

            SendButtonImageResponse(request.Socket, imageId, imageAsBytes);
        }

        private void HandleRequestStatValue(TcpRequest request) {
            var statValueRequest = JsonConvert.DeserializeObject<StatValueRequest>(request.Body);

            _logger.LogMessage($"Handling Stat Value request");

            var player = GetPlayer(statValueRequest.Deck);
            var stat = GetStat(player, statValueRequest.StatType);

            var response = new StatValueResponse {
                Value = stat.Value
            };

            Send(request.Socket, response.ToString());
        }

        private void HandleChangeStatValue(TcpRequest request) {
            var changeStatValueRequest = JsonConvert.DeserializeObject<ChangeStatValueRequest>(request.Body);

            _logger.LogMessage($"Handling Change Stat Value request");

            var player = GetPlayer(changeStatValueRequest.Deck);
            var stat = GetStat(player, changeStatValueRequest.StatType);
            if (changeStatValueRequest.Increase) {
                stat.Increase.Execute(null);
            } else {
                stat.Decrease.Execute(null);
            }

            var response = new StatValueResponse {
                Value = stat.Value
            };

            Send(request.Socket, response.ToString());
        }

        private IButton GetCardButton(IButtonContext context) {
            var cardGroup = _appData.Game.GetCardGroup(context.CardGroupId);
            return cardGroup.GetButton(context);
        }

        private void HandleRegisterForUpdates(TcpRequest request) {
            _logger.LogMessage("Handling register for update request");

            var registerForUpdatesRequest = JsonConvert.DeserializeObject<RegisterForUpdatesRequest>(request.Body);

            _broadcastService.AddPort(registerForUpdatesRequest.Port);

            SendAllStats();

            var cardGroupInfoList = new List<CardGroupInfo>();
            var buttonInfoList = new List<ButtonInfo>();
            foreach (var cardGroup in _appData.Game.AllCardGroups) {
                cardGroupInfoList.Add(new CardGroupInfo {
                    CardGroupId = cardGroup.Id,
                    Name = cardGroup.Name,
                    IsImageAvailable  = cardGroup.ButtonImageAsBytes != null,
                    ImageId  = cardGroup.Name,
                    Zones = (from zone in cardGroup.CardZones select zone.Name).ToList()
                });

                var poolButtonIndex = 0;
                foreach (var button in cardGroup.CardButtons) {
                    buttonInfoList.Add(CreateButtonInfo(cardGroup.Id, ButtonMode.Pool, 0, poolButtonIndex++, button));
                }

                foreach (var zone in cardGroup.CardZones) {
                    var zoneButtonIndex = 0;
                    foreach (var button in zone.Buttons) {
                        buttonInfoList.Add(CreateButtonInfo(cardGroup.Id, ButtonMode.Zone, zone.ZoneIndex, zoneButtonIndex, button));
                    }
                }
            }

            SendRegisterForUpdatesResponse(request.Socket, cardGroupInfoList, buttonInfoList);
        }

        private static ButtonInfo CreateButtonInfo(CardGroupId cardGroupId, ButtonMode buttonMode, int zoneIndex, int index, IButton button) {
            var cardImageButton = button as CardImageButton;

            return new ButtonInfo {
                CardGroupId = cardGroupId,
                ButtonMode = buttonMode,
                ZoneIndex = zoneIndex,
                Index = index,
                Name = button.Text,
                Code = cardImageButton == null ? string.Empty : cardImageButton.CardInfo.Code,
                IsToggled = button.IsToggled,
                ImageAvailable = cardImageButton != null && cardImageButton.CardInfo.ButtonImageAsBytes != null,
                ButtonOptions = button.Options
            };
        }

        private void SendAllStats() {
            var game = _appData.Game;
            foreach (var player in game.Players) {
                _eventBus.PublishStatUpdated(player.CardGroup.Id, StatType.Health, player.Health.Value);
                _eventBus.PublishStatUpdated(player.CardGroup.Id, StatType.Sanity, player.Sanity.Value);
                _eventBus.PublishStatUpdated(player.CardGroup.Id, StatType.Resources, player.Resources.Value);
                _eventBus.PublishStatUpdated(player.CardGroup.Id, StatType.Clues, player.Clues.Value);
            }
        }

        private void SendCardInfoResponse(Socket socket, IButton cardButton) {
            var cardImageButton = cardButton as CardImageButton;

            var cardInfoReponse = (cardButton == null)
                ? new CardInfoResponse { CardButtonType = CardButtonType.Unknown, Name = "" }
                : new CardInfoResponse {
                    CardButtonType = GetCardType(cardImageButton?.CardInfo),
                    Name = cardButton.Text.Replace("Right Click", "Long Press"),
                    IsToggled = cardButton.IsToggled,
                    Code = cardImageButton?.CardInfo.ImageId,
                    ImageAvailable = cardImageButton?.CardInfo.ButtonImageAsBytes != null,
                    ButtonOptions = cardButton.Options
                };

            Send(socket, cardInfoReponse.ToString());
        }

        private void SendButtonImageResponse(Socket socket, string code, byte[] imageAsBytes) {
            var buttonImageResponse = new ButtonImageResponse { 
                Code = code, 
                Bytes = imageAsBytes
            };

            Send(socket, buttonImageResponse.ToString());
        }

        private void SendRegisterForUpdatesResponse(Socket socket, IList<CardGroupInfo> cardGroupInfo, IList<ButtonInfo> buttonInfoList) {
            var registerForUpdatesResponse = new RegisterForUpdatesResponse {
                CardGroupInfo = cardGroupInfo,
                Buttons = buttonInfoList
            };

            Send(socket, registerForUpdatesResponse.ToString());
        }

        private void SendOkResponse(Socket socket) {
            Send(socket, new OkResponse().ToString());
        }

        private Player GetPlayer(CardGroupId cardGroup) {
            switch (cardGroup) {
                case CardGroupId.Player1:
                    return _appData.Game.Players[0];
                case CardGroupId.Player2:
                    return _appData.Game.Players[1];
                case CardGroupId.Player3:
                    return _appData.Game.Players[2];
                case CardGroupId.Player4:
                    return _appData.Game.Players[3];
                default:
                    return _appData.Game.Players[0];
            }
        }

        private Stat GetStat(Player player, StatType statType) {
            switch (statType) {
                case StatType.Health:
                    return player.Health;
                case StatType.Sanity:
                    return player.Sanity;
                case StatType.Resources:
                    return player.Resources;
                case StatType.Clues:
                    return player.Clues;
                default:
                    return player.Health;
            }
        }

        private static CardButtonType GetCardType(CardInfo card) {
            if (card == null) {
                return CardButtonType.Action;
            }

            switch (card.Type) {
                case CardType.Scenario:
                    return CardButtonType.Scenario;
                case CardType.Agenda:
                    return CardButtonType.Agenda;
                case CardType.Act:
                    return CardButtonType.Act;
                case CardType.Location:
                    return CardButtonType.Location;
                case CardType.Enemy:
                    return CardButtonType.Enemy;
                case CardType.Treachery:
                    return CardButtonType.Treachery;
                case CardType.Asset:
                    return CardButtonType.Asset;
                case CardType.Event:
                    return CardButtonType.Event;
                case CardType.Skill:
                    return CardButtonType.Skill;
                default:
                    return CardButtonType.Unknown;
            }
        }

        private void Send(Socket socket, string data) {
            var byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            try {
                socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), socket);
            } catch (Exception ex) {
                _logger.LogException(ex, "Error sending message to remote server");
            }
        }

        private static void SendCallback(IAsyncResult ar) {
            try {
                var socket = (Socket)ar.AsyncState;

                var bytesSent = socket.EndSend(ar);
                // TODO: Log this
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            } catch (Exception ex) {
                // TODO: Log exception
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
