using ArkhamOverlay.Data;
using ArkhamOverlay.Pages.Main;
using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ArkhamOverlay.Services {
    internal class TcpRequestHandler : IRequestHandler {
        private readonly AppData _appData;

        public TcpRequestHandler(AppData viewModel) {
            _appData = viewModel;
        }

        public void HandleRequest(TcpRequest request) {
            Console.WriteLine("Handling Request: " + request.RequestType.AsString());
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
                case AoTcpRequest.ClickCardButton:
                    HandleClick(request);
                    break;
                case AoTcpRequest.RegisterForUpdates:
                    HandleRegisterForUpdates(request);
                    break;
            }
        }

        private void HandleGetCardInfo(TcpRequest request) {
            var getCardInfoRequest = JsonConvert.DeserializeObject<GetCardInfoRequest>(request.Body);
            var cardIndex = getCardInfoRequest.Index;

            var cards = GetDeck(getCardInfoRequest.Deck).CardButtons;

            var cardButton = (cardIndex < cards.Count) ? cards[cardIndex] : null;
            SendCardInfoResponse(request.Socket, cardButton);
        }

        private void HandleClick(TcpRequest request) {
            var clickCardButtonRequest = JsonConvert.DeserializeObject<ClickCardButtonRequest>(request.Body);
            var cardIndex = clickCardButtonRequest.Index;

            var cards = GetDeck(clickCardButtonRequest.Deck).CardButtons;

            var cardButton = (cardIndex < cards.Count) ? cards[cardIndex] : null;
            cardButton.Click();

            SendCardInfoResponse(request.Socket, cardButton);
        }

        private void HandleClearAll(TcpRequest request) {
            _appData.Game.ClearAllCards();
            SendOkResponse(request.Socket);
        }

        private void HandleToggleActAgendaBar(TcpRequest request) {
            _appData.IsActAgendaBarVisible = !_appData.IsActAgendaBarVisible;
            SendOkResponse(request.Socket);
        }

        private readonly IList<int> _updatePorts = new List<int>();
        private bool _alreadyRegisteredEvents = false;

        private object _registerLock = new object();
        private void HandleRegisterForUpdates(TcpRequest request) {
            var registerForUpdatesRequest = JsonConvert.DeserializeObject<RegisterForUpdatesRequest>(request.Body);
            if (!_updatePorts.Contains(registerForUpdatesRequest.Port)) {
                _updatePorts.Add(registerForUpdatesRequest.Port);
            }

            lock(_registerLock) {
                if (!_alreadyRegisteredEvents) {
                    _appData.PropertyChanged += (s, e) => {
                        if (e.PropertyName == nameof(AppData.IsActAgendaBarVisible)) {
                            SendActAgendaBarStatus(_appData.IsActAgendaBarVisible);
                        }
                    };

                    var game = _appData.Game;
                    foreach (var selectableCards in game.AllSelectableCards) {
                        selectableCards.CardToggled += (card1, card2) => {
                            SendCardInfoUpdate(card1, selectableCards);
                            if (card2 != null) {
                                SendCardInfoUpdate(card2, selectableCards);
                            }
                        };
                    }

                    _alreadyRegisteredEvents = true;
                }
            }

            SendOkResponse(request.Socket);
        }

        private void SendActAgendaBarStatus(bool isVisible) {
            var request = new ActAgendaBarStatusRequest {
                IsVisible = isVisible
            };
            SendStatusToAllRegisteredPorts(request);
        }

        private void SendCardInfoUpdate(Card card, SelectableCards selectableCards) {
            var deck = GetDeckType(selectableCards);
            var request = new UpdateCardInfoRequest {
                Deck = deck,
                Index = selectableCards.CardButtons.IndexOf(card),
                CardButtonType = GetCardType(card),
                Name = card.Name,
                ImageSource = card.ImageSource,
                IsVisible = card.IsVisible
            };

            SendStatusToAllRegisteredPorts(request);
        }

        private void SendStatusToAllRegisteredPorts(Request request) {
            //if no one is listening, don't speak!
            if (!_updatePorts.Any()) {
                return;
            }

            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                var portsToRemove = new List<int>();
                foreach (var port in _updatePorts.ToList()) {
                    try {
                        SendSocketService.SendRequest(request, port);
                    } catch {
                        //this clearly is not cool- stop trying to talk
                        portsToRemove.Add(port);
                    }
                }

                foreach (var portToRemove in portsToRemove) {
                    _updatePorts.Remove(portToRemove);
                }
            };
            worker.RunWorkerAsync();
        }

        private Deck GetDeckType(SelectableCards selectableCards) {
            var game = _appData.Game;
            if (selectableCards == game.ScenarioCards) {
                return Deck.Scenario;
            }
            if (selectableCards == game.LocationCards) {
                return Deck.Locations;
            }
            if (selectableCards == game.EncounterDeckCards) {
                return Deck.EncounterDeck;
            }
            if (selectableCards == game.Players[0].SelectableCards) {
                return Deck.Player1;
            }
            if (selectableCards == game.Players[1].SelectableCards) {
                return Deck.Player2;
            }
            if (selectableCards == game.Players[2].SelectableCards) {
                return Deck.Player3;
            }
            return Deck.Player4;
        }

        private void SendCardInfoResponse(Socket socket, ICardButton cardButton) {
            var card = (cardButton as Card);
            var cardInfoReponse = (cardButton == null)
                ? new CardInfoResponse { CardButtonType = CardButtonType.Unknown, Name = "" }
                : new CardInfoResponse { 
                    CardButtonType = GetCardType(card), 
                    Name = cardButton.Name, 
                    IsVisible = card != null ? card.IsVisible : false,
                    ImageSource = card != null ? card.ImageSource : string.Empty };

            Send(socket, cardInfoReponse.ToString());
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

        private static CardButtonType GetCardType(Card card) {
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

        private static void Send(Socket socket, string data) {
            var byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), socket);
        }

        private static void SendCallback(IAsyncResult ar) {
            try {
                var socket = (Socket)ar.AsyncState;

                var bytesSent = socket.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
