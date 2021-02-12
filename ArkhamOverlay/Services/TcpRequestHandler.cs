using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Events;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Tcp;
using ArkhamOverlay.Common.Tcp.Requests;
using ArkhamOverlay.Common.Tcp.Responses;
using ArkhamOverlay.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Threading;

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
            _logger.LogMessage($"Handling Request: {request.RequestType.ToString()}");
            switch (request.RequestType) {
                case AoTcpRequest.ClearAll:
                    HandleClearAll(request);
                    break;
                case AoTcpRequest.ToggleActAgendaBarRequest:
                    HandleToggleActAgendaBar(request);
                    break;
                case AoTcpRequest.GetCardInfo:
                    HandleGetCardInfo(request);
                    break;
                case AoTcpRequest.GetButtonImage:
                    HandleGetButtonImage(request);
                    break;
                case AoTcpRequest.ClickCardButton:
                    HandleClick(request);
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
                case AoTcpRequest.GetInvestigatorImage:
                    HandleGetInvesigatorImageRequest(request);
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

        private void HandleGetCardInfo(TcpRequest request) {
            _logger.LogMessage("Handling card info request");
            var getCardInfoRequest = JsonConvert.DeserializeObject<GetCardInfoRequest>(request.Body);
            var cardButton = GetCardButton(getCardInfoRequest.Deck, getCardInfoRequest.FromCardSet, getCardInfoRequest.Index);
            SendCardInfoResponse(request.Socket, cardButton);
        }

        private void HandleGetButtonImage(TcpRequest request) {
            _logger.LogMessage("Handling button image request");
            var buttonImageRequest = JsonConvert.DeserializeObject<ButtonImageRequest>(request.Body);
            var cardButton = GetCardButton(buttonImageRequest.Deck, buttonImageRequest.FromCardSet, buttonImageRequest.Index);
            SendButtonImageResponse(request.Socket, cardButton as CardTemplateButton);
        }

        private void HandleClick(TcpRequest request) {
            var clickCardButtonRequest = JsonConvert.DeserializeObject<ClickCardButtonRequest>(request.Body);
            _logger.LogMessage($"Handling {clickCardButtonRequest.Click} request");
            var cardButton = GetCardButton(clickCardButtonRequest.Deck, clickCardButtonRequest.FromCardSet, clickCardButtonRequest.Index);
            if (cardButton == null) {
                SendOkResponse(request.Socket);
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                if (clickCardButtonRequest.Click == ButtonClick.Left) {
                    cardButton.LeftClick();
                } else {
                    cardButton.RightClick();
                }
                SendOkResponse(request.Socket);
            }));
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

        private ICardButton GetCardButton(Deck deck, bool cardInSet,  int index) {
            var selectableCards = GetDeck(deck);
            if (cardInSet) {
                return (index < selectableCards.CardSet.Buttons.Count) ? selectableCards.CardSet.Buttons[index] : null;
            } 
            return (index < selectableCards.CardButtons.Count) ? selectableCards.CardButtons[index] : null;
        }

        private void HandleClearAll(TcpRequest request) {
            _logger.LogMessage("Handling clear all request");
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                _appData.Game.ClearAllCards();
                SendOkResponse(request.Socket);
            }));
        }

        private void HandleToggleActAgendaBar(TcpRequest request) {
            _logger.LogMessage("Handling toggle act/agenda bar request");
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                _appData.Game.ScenarioCards.ToggleCardSetVisibility();
                SendOkResponse(request.Socket);
            }));
        }

        private bool _alreadyRegisteredEvents = false;

        private object _registerLock = new object();
        private void HandleRegisterForUpdates(TcpRequest request) {
            _logger.LogMessage("Handling register for update request");
            var registerForUpdatesRequest = JsonConvert.DeserializeObject<RegisterForUpdatesRequest>(request.Body);
            if (!_broadcastService.Ports.Contains(registerForUpdatesRequest.Port)) {
                _broadcastService.Ports.Add(registerForUpdatesRequest.Port);
            }
            
            lock (_registerLock) {
                if (!_alreadyRegisteredEvents) {
                    var game = _appData.Game;
                    foreach (var player in game.Players) {
                        player.PropertyChanged += (s, e) => {
                            if (e.PropertyName == nameof(Player.ButtonImageAsBytes)) {
                                SendInvestigatorImage(player);
                            }
                        };
                    }

                    foreach (var selectableCards in game.AllSelectableCards) {
                        selectableCards.ButtonChanged += (button) => {
                            if (button is CardInSetButton cardInSetButton) {
                                SendCardInSetInfoUpdate(cardInSetButton, selectableCards);
                                return;
                            }

                            SendButtonInfoUpdate(button, selectableCards);
                        };

                        selectableCards.CardSet.Buttons.CollectionChanged += (s, e) => {
                            foreach (var button in selectableCards.CardSet.Buttons) {
                                SendCardInSetInfoUpdate(button, selectableCards);
                            }

                            //if an item was removed, we need to send an update to clear the last item
                            if (e.Action == NotifyCollectionChangedAction.Remove) {
                                SendCardInSetInfoUpdate(null, selectableCards);
                            }
                        };
                    }

                    SendAllStats();
                    SendAllInvestigatorImages();

                    _alreadyRegisteredEvents = true;
                }
            }

            SendOkResponse(request.Socket);
        }

        private void SendAllStats() {
            var game = _appData.Game;
            foreach (var player in game.Players) {
                _eventBus.PublishStatUpdatedEvent(GetDeckType(player.SelectableCards), StatType.Health, player.Health.Value);
                _eventBus.PublishStatUpdatedEvent(GetDeckType(player.SelectableCards), StatType.Sanity, player.Sanity.Value);
                _eventBus.PublishStatUpdatedEvent(GetDeckType(player.SelectableCards), StatType.Resources, player.Resources.Value);
                _eventBus.PublishStatUpdatedEvent(GetDeckType(player.SelectableCards), StatType.Clues, player.Clues.Value);
            }
        }

        private void SendAllInvestigatorImages() {
            var game = _appData.Game;
            foreach (var player in game.Players) {
                SendInvestigatorImage(player);
            }
        }

        private void HandleGetInvesigatorImageRequest(TcpRequest request) {
            _logger.LogMessage("Handling get invetsigator image request");
            SendOkResponse(request.Socket);

            var getInvestigatorImageRequest = JsonConvert.DeserializeObject<GetInvestigatorImageRequest>(request.Body);
            var player = GetPlayer(getInvestigatorImageRequest.Deck);
            SendInvestigatorImage(player);
        }

        private void SendButtonInfoUpdate(ICardButton button, SelectableCards selectableCards) {
            _logger.LogMessage("Sending button info update");

            CardTemplate card = null;
            if (button is CardImageButton cardImageButton) {
                card = cardImageButton.CardTemplate;
            }

            var deck = GetDeckType(selectableCards);

            var request = new UpdateCardInfoRequest {
                Deck = deck,
                Index = selectableCards.CardButtons.IndexOf(button),
                CardButtonType = GetCardType(card),
                Name = button?.Text,
                IsToggled = button != null && button.IsToggled,
                ImageAvailable = card?.ButtonImageAsBytes != null,
                IsCardInSet = false
            };

            //this sometimes happen when a button isn't in a list yet (on create)
            ///TODO: there's probably a cleaner way
            if (request.Index == -1) {
                return;               
            }

            _broadcastService.SendRequest(request);
        }

        private void SendCardInSetInfoUpdate(CardInSetButton button, SelectableCards selectableCards) {
            CardTemplate card = null;
            if (button is CardImageButton cardImageButton) {
                card = cardImageButton.CardTemplate;
            }

            var deck = GetDeckType(selectableCards);

            var request = new UpdateCardInfoRequest {
                Deck = deck,
                Index = button == null ? selectableCards.CardSet.Buttons.Count : selectableCards.CardSet.Buttons.IndexOf(button),
                CardButtonType = GetCardType(card),
                Name = button?.Text.Replace("Right Click", "Long Press"),
                IsToggled = button != null && button.IsToggled,
                ImageAvailable = card?.ButtonImageAsBytes != null,
                IsCardInSet = true
            };

            //this sometimes happen when a button isn't in a list yet (on create)
            ///TODO: there's probably a cleaner way
            if (request.Index == -1) {
                return;
            }

            _broadcastService.SendRequest(request);
        }

        private void SendInvestigatorImage(Player player) {
            _logger.LogMessage("Sending Investigator Image");

            var deck = GetDeckType(player.SelectableCards);
            var request = new UpdateInvestigatorImageRequest {
                Deck = deck,
                Bytes = player.ButtonImageAsBytes
            };

            _broadcastService.SendRequest(request);
        }

        private Deck GetDeckType(SelectableCards selectableCards) {
            var game = _appData.Game;
            switch(selectableCards.Type) {
                case SelectableType.Scenario:
                    return Deck.Scenario;
                case SelectableType.Encounter:
                    return Deck.EncounterDeck;
                case SelectableType.Location:
                    return Deck.Locations;
                case SelectableType.Player:
                    if (selectableCards == game.Players[0].SelectableCards) {
                        return Deck.Player1;
                    } else if (selectableCards == game.Players[1].SelectableCards) {
                        return Deck.Player2;
                    } else if (selectableCards == game.Players[2].SelectableCards) {
                        return Deck.Player3;
                    } else if (selectableCards == game.Players[3].SelectableCards) {
                        return Deck.Player4;
                    } else {
                        _logger.LogWarning($"Unrecognized SelectableCards player: {selectableCards.Name}.");
                        return Deck.Player1;
                    }
                default:
                    _logger.LogWarning($"Unrecognized SelectableCards: {selectableCards.Name}.");
                    return Deck.Player1;
            }
        }

        private void SendCardInfoResponse(Socket socket, ICardButton cardButton) {
            var cardImageButton = cardButton as CardImageButton;

            var cardInfoReponse = (cardButton == null)
                ? new CardInfoResponse { CardButtonType = CardButtonType.Unknown, Name = "" }
                : new CardInfoResponse { 
                    CardButtonType = GetCardType(cardImageButton?.CardTemplate),
                    Name = cardButton.Text.Replace("Right Click", "Long Press"),
                    IsToggled = cardButton.IsToggled,
                    ImageAvailable = cardImageButton?.CardTemplate.ButtonImageAsBytes != null
                };

            Send(socket, cardInfoReponse.ToString());
        }

        private void SendButtonImageResponse(Socket socket, CardTemplateButton cardButton) {
            var buttonImageResponse = new ButtonImageResponse { 
                Name = cardButton?.CardTemplate.Name, 
                Bytes = cardButton?.CardTemplate.ButtonImageAsBytes 
            };

            Send(socket, buttonImageResponse.ToString());
        }


        private void SendOkResponse(Socket socket) {
            Send(socket, new OkResponse().ToString());
        }

        private SelectableCards GetDeck(Deck deck) {
            switch (deck) {
                case Deck.Player1: 
                    return _appData.Game.Players[0].SelectableCards;
                case Deck.Player2: 
                    return _appData.Game.Players[1].SelectableCards;
                case Deck.Player3:
                    return _appData.Game.Players[2].SelectableCards;
                case Deck.Player4:
                    return _appData.Game.Players[3].SelectableCards;
                case Deck.Scenario:
                    return _appData.Game.ScenarioCards;
                case Deck.Locations:
                    return _appData.Game.LocationCards;
                case Deck.EncounterDeck:
                    return _appData.Game.EncounterDeckCards;
                default:
                    return _appData.Game.Players[0].SelectableCards;
            }
        }

        private Player GetPlayer(Deck deck) {
            switch (deck) {
                case Deck.Player1:
                    return _appData.Game.Players[0];
                case Deck.Player2:
                    return _appData.Game.Players[1];
                case Deck.Player3:
                    return _appData.Game.Players[2];
                case Deck.Player4:
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

        private static CardButtonType GetCardType(CardTemplate card) {
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
