using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using Newtonsoft.Json;
using StreamDeckPlugin.Actions;
using System;
using System.Net.Sockets;
using System.Text;

namespace StreamDeckPlugin.Utils {
    public class TcpRequestHandler : IRequestHandler {
        public bool RequestReceivedRecently { get; set; } 

        public void HandleRequest(TcpRequest request) {
            RequestReceivedRecently = true;

            Console.WriteLine("Handling Request: " + request.RequestType.AsString());
            switch (request.RequestType) {
                case AoTcpRequest.UpdateCardInfo:
                    UpdateCardInfo(request);
                    break;
                case AoTcpRequest.UpdateStatInfo:
                    UpdateStatInfo(request);
                    break;
                case AoTcpRequest.UpdateInvestigatorImage:
                    UpdateInvestigatorImage(request);
                    break;
                case AoTcpRequest.ActAgendaBarStatusRequest:
                    UpdateActAgendaBarStatus(request);
                    break;
            }
        }

        private void UpdateCardInfo(TcpRequest request) {
            var updateCardInfoRequest = JsonConvert.DeserializeObject<UpdateCardInfoRequest>(request.Body);
            if (updateCardInfoRequest == null) {
                return;
            }

            foreach (var cardButtonAction in CardButtonAction.ListOf) {
                if ((cardButtonAction.Deck == updateCardInfoRequest.Deck) &&
                    (cardButtonAction.CardButtonIndex == updateCardInfoRequest.Index) &&
                    cardButtonAction.IsVisible && 
                    cardButtonAction.ShowCardSet == updateCardInfoRequest.IsCardInSet) {
#pragma warning disable CS4014 
                    cardButtonAction.UpdateButtonInfo(updateCardInfoRequest);
#pragma warning restore CS4014 
                }
            }
            Send(request.Socket, new OkResponse().ToString());
        }

        private void UpdateStatInfo(TcpRequest request) {
            var updateStatInfoRequest = JsonConvert.DeserializeObject<UpdateStatInfoRequest>(request.Body);
            if (updateStatInfoRequest == null) {
                return;
            }

            foreach (var trackStatAction in TrackStatAction.ListOf) {
                if ((trackStatAction.Deck == updateStatInfoRequest.Deck) &&
                    (trackStatAction.StatType == updateStatInfoRequest.StatType)) { 
                    trackStatAction.UpdateValue(updateStatInfoRequest.Value);
                }
            }
            Send(request.Socket, new OkResponse().ToString());
        }

        private void UpdateInvestigatorImage(TcpRequest request) {
            var updateInvestigatorImageRequest = JsonConvert.DeserializeObject<UpdateInvestigatorImageRequest>(request.Body);
            if (updateInvestigatorImageRequest == null) {
                return;
            }

            foreach (var showDeckListAction in ShowDeckListAction.ListOf) {
                if (showDeckListAction.Deck == updateInvestigatorImageRequest.Deck) {
                    showDeckListAction.SetImageAsync(updateInvestigatorImageRequest.Bytes.AsImage());
                }
            }
            Send(request.Socket, new OkResponse().ToString());
        }

        private void UpdateActAgendaBarStatus(TcpRequest request) {
            var actAgendaBarStatusRequest = JsonConvert.DeserializeObject<ActAgendaBarStatusRequest>(request.Body);
            if (actAgendaBarStatusRequest == null) {
                return;
            }

            foreach (var toggleActAgendaBarAction in ToggleActAgendaBarAction.ListOf) {
                toggleActAgendaBarAction.SetStatus(actAgendaBarStatusRequest.IsVisible);
            }
            Send(request.Socket, new OkResponse().ToString());
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
