using Newtonsoft.Json;
using StreamDeckPlugin.Events;
using StreamDeckPlugin.Services;
using System;
using System.Net.Sockets;
using System.Text;
using ArkhamOverlay.Common.Tcp;
using ArkhamOverlay.Common.Tcp.Requests;
using ArkhamOverlay.Common.Tcp.Responses;

namespace StreamDeckPlugin.Utils {
    public class TcpRequestHandler : IRequestHandler {
        private readonly IDynamicActionInfoStore _dynamicActionService;
        private readonly IEventBus _eventBus;

        public TcpRequestHandler(IDynamicActionInfoStore dynamicActionService, IEventBus eventBus) {
            _dynamicActionService = dynamicActionService;
            _eventBus = eventBus;
        }

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
            }
        }

        private void UpdateCardInfo(TcpRequest request) {
            var updateCardInfoRequest = JsonConvert.DeserializeObject<UpdateCardInfoRequest>(request.Body);
            if (updateCardInfoRequest != null) {
                var mode = updateCardInfoRequest.IsCardInSet ? DynamicActionMode.Set : DynamicActionMode.Pool;
                _dynamicActionService.UpdateDynamicActionInfo(updateCardInfoRequest.Deck, updateCardInfoRequest.Index, mode, updateCardInfoRequest);
            }
            Send(request.Socket, new OkResponse().ToString());
        }

        private void UpdateStatInfo(TcpRequest request) {
            var updateStatInfoRequest = JsonConvert.DeserializeObject<UpdateStatInfoRequest>(request.Body);
            if (updateStatInfoRequest != null) {
                _eventBus.PublishStatUpdatedEvent(updateStatInfoRequest.Deck, updateStatInfoRequest.StatType, updateStatInfoRequest.Value);
            }

            Send(request.Socket, new OkResponse().ToString());
        }

        private void UpdateInvestigatorImage(TcpRequest request) {
            var updateInvestigatorImageRequest = JsonConvert.DeserializeObject<UpdateInvestigatorImageRequest>(request.Body);
            if (updateInvestigatorImageRequest != null) {
                _eventBus.PublishInvestigatorImageUpdatedEvent(updateInvestigatorImageRequest.Deck, updateInvestigatorImageRequest.Bytes);
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
