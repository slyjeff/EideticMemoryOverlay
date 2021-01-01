using ArkhamOverlay.Data;
using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;

namespace ArkhamOverlay.Services {
    internal class RequestHandler {
        private readonly AppData _appData;

        public RequestHandler(AppData appData) {
            _appData = appData;
        }

        public void HandleRequest(TcpRequest request) {
            Console.WriteLine("Handling Request: " + request.RequestType.AsString());
            if (request.RequestType == AoTcpRequest.GetCardInfo) {
                HandleGetCardInfo(request);
            }

            if (request.RequestType == AoTcpRequest.ClickCardButton) {
                HandleClick(request);
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

        private void SendCardInfoResponse(Socket socket, ICardButton cardButton) {
            var card = (cardButton as Card);
            var cardInfoReponse = (cardButton == null)
                ? new CardInfoResponse { CardButtonType = CardButtonType.Unknown, Name = "" }
                : new CardInfoResponse { CardButtonType = GetCardType(card), Name = cardButton.Name, ImageSource = card != null ? card.ImageSource : string.Empty };

            Send(socket, cardInfoReponse.ToString());
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
