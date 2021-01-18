using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Data;
using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Threading;

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
                case AoTcpRequest.GetButtonImage:
                    HandleGetButtonImage(request);
                    break;
                case AoTcpRequest.ClickCardButton:
                    HandleClick(request);
                    break;
                case AoTcpRequest.RegisterForUpdates:
                    HandleRegisterForUpdates(request);
                    break;
                case AoTcpRequest.ShowDeckList:
                    HandleShowDeckList(request);
                    break;
            }
        }

        private void HandleGetCardInfo(TcpRequest request) {
            var getCardInfoRequest = JsonConvert.DeserializeObject<GetCardInfoRequest>(request.Body);
            var cardButton = GetCardButton(getCardInfoRequest.Deck, getCardInfoRequest.FromCardSet, getCardInfoRequest.Index);
            SendCardInfoResponse(request.Socket, cardButton);
        }

        private void HandleGetButtonImage(TcpRequest request) {
            var buttonImageRequest = JsonConvert.DeserializeObject<ButtonImageRequest>(request.Body);
            var cardButton = GetCardButton(buttonImageRequest.Deck, buttonImageRequest.FromCardSet, buttonImageRequest.Index);
            SendButtonImageResponse(request.Socket, cardButton as ShowCardButton);
        }

        private void HandleClick(TcpRequest request) {
            var clickCardButtonRequest = JsonConvert.DeserializeObject<ClickCardButtonRequest>(request.Body);
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

        private ICardButton GetCardButton(Deck deck, bool cardInSet,  int index) {
            var selectableCards = GetDeck(deck);
            if (cardInSet) {
                return (index < selectableCards.CardSet.Buttons.Count) ? selectableCards.CardSet.Buttons[index] : null;
            } 
            return (index < selectableCards.CardButtons.Count) ? selectableCards.CardButtons[index] : null;
        }

        private void HandleClearAll(TcpRequest request) {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                _appData.Game.ClearAllCards();
                SendOkResponse(request.Socket);
            }));
        }

        private void HandleToggleActAgendaBar(TcpRequest request) {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                _appData.Game.ScenarioCards.ToggleCardSetVisibility();
                SendOkResponse(request.Socket);
            }));
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
                    var game = _appData.Game;
                    game.LocationCards.CardSet.Buttons.CollectionChanged += (s, e) => {
                        SendActAgendaBarStatus(game.LocationCards.CardSet.IsDisplayedOnOverlay);
                    };

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

                    _alreadyRegisteredEvents = true;
                }
            }

            SendOkResponse(request.Socket);
        }

        private void HandleShowDeckList(TcpRequest request) {
            var showDeckListRequest = JsonConvert.DeserializeObject<ShowDeckListRequest>(request.Body);
            var selectableCards = GetDeck(showDeckListRequest.Deck);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                selectableCards.ShowDeckList();

                SendOkResponse(request.Socket);
            }));
        }

        private void SendActAgendaBarStatus(bool isVisible) {
            var request = new ActAgendaBarStatusRequest {
                IsVisible = isVisible
            };
            SendStatusToAllRegisteredPorts(request);
        }

        private void SendButtonInfoUpdate(ICardButton button, SelectableCards selectableCards) {
            Card card = null;
            if (button is CardImageButton cardImageButton) {
                card = cardImageButton.Card;
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

            SendStatusToAllRegisteredPorts(request);
        }

        private void SendCardInSetInfoUpdate(CardInSetButton button, SelectableCards selectableCards) {
            Card card = null;
            if (button is CardImageButton cardImageButton) {
                card = cardImageButton.Card;
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
            var cardImageButton = cardButton as CardImageButton;

            var cardInfoReponse = (cardButton == null)
                ? new CardInfoResponse { CardButtonType = CardButtonType.Unknown, Name = "" }
                : new CardInfoResponse { 
                    CardButtonType = GetCardType(cardImageButton?.Card),
                    Name = cardButton.Text.Replace("Right Click", "Long Press"),
                    IsToggled = cardButton.IsToggled,
                    ImageAvailable = cardImageButton?.Card.ButtonImageAsBytes != null
                };

            Send(socket, cardInfoReponse.ToString());
        }

        private void SendButtonImageResponse(Socket socket, ShowCardButton cardButton) {
            var buttonImageResponse = new ButtonImageResponse { 
                Name = cardButton?.Card.Name, 
                Bytes = cardButton?.Card.ButtonImageAsBytes 
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
