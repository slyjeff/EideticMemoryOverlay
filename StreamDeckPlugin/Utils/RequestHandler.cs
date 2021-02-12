﻿using Newtonsoft.Json;
using StreamDeckPlugin.Events;
using StreamDeckPlugin.Services;
using System;
using System.Net.Sockets;
using System.Text;
using ArkhamOverlay.Common.Tcp;
using ArkhamOverlay.Common.Tcp.Requests;
using ArkhamOverlay.Common.Tcp.Responses;
using ArkhamOverlay.Common.Services;

namespace StreamDeckPlugin.Utils {
    public class TcpRequestHandler : IRequestHandler {
        private readonly IDynamicActionInfoStore _dynamicActionService;
        private readonly IEventBus _eventBus;
        private readonly ICrossAppEventBus _crossAppEventBus;

        public TcpRequestHandler(IDynamicActionInfoStore dynamicActionService, IEventBus eventBus, ICrossAppEventBus crossAppEventBus) {
            _dynamicActionService = dynamicActionService;
            _eventBus = eventBus;
            _crossAppEventBus = crossAppEventBus;
        }

        public bool RequestReceivedRecently { get; set; } 

        public void HandleRequest(TcpRequest request) {
            RequestReceivedRecently = true;

            Console.WriteLine("Handling Request: " + request.RequestType.ToString());
            switch (request.RequestType) {
                case AoTcpRequest.UpdateCardInfo:
                    UpdateCardInfo(request);
                    break;
                case AoTcpRequest.UpdateInvestigatorImage:
                    UpdateInvestigatorImage(request);
                    break;
                case AoTcpRequest.EventBus:
                    HandleEventBusRequest(request);
                    break;
            }
        }

        private void UpdateCardInfo(TcpRequest request) {
            var updateCardInfoRequest = JsonConvert.DeserializeObject<UpdateCardInfoRequest>(request.Body);
            if (updateCardInfoRequest != null) {
                _dynamicActionService.UpdateDynamicActionInfo(updateCardInfoRequest, updateCardInfoRequest);
            }
            Send(request.Socket, new OkResponse().ToString());
        }

        private void UpdateInvestigatorImage(TcpRequest request) {
            var updateInvestigatorImageRequest = JsonConvert.DeserializeObject<UpdateInvestigatorImageRequest>(request.Body);
            if (updateInvestigatorImageRequest != null) {
                _eventBus.PublishInvestigatorImageUpdatedEvent(updateInvestigatorImageRequest.CardGroup, updateInvestigatorImageRequest.Bytes);
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

        private void HandleEventBusRequest(TcpRequest request) {
            Send(request.Socket, new OkResponse().ToString());

            var eventBusRequest = JsonConvert.DeserializeObject<EventBusRequest>(request.Body);
            _crossAppEventBus.ReceiveMessage(eventBusRequest);
        }
    }
}
