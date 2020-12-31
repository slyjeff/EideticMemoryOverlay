using ArkhamOverlay.Data;
using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.TcpUtils.Requests;
using ArkhamOverlay.TcpUtils.Responses;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ArkhamOverlay.Services {
    internal class StateObject {
        public const int BufferSize = 1024;

        public byte[] Buffer = new byte[BufferSize];

        public StringBuilder Data = new StringBuilder();

        public Socket Socket = null;

        public SocketService Service;
    }

    internal class TcpRequest {
        public TcpRequest(string request, Socket socket) {
            var endOfCode = request.IndexOf(":") - 1;
            var startOfBody = endOfCode + 1;
            var endOfBody = request.IndexOf("<EOF>") - 2;
            var bodyLength = endOfBody - startOfBody;

            var code = request.Substring(0, endOfCode + 1);
            RequestType = code.AsAoTcpRequest();
            Body = request.Substring(startOfBody, bodyLength);
        }

        public AoTcpRequest RequestType { get; }
        public string Body { get; }
        public Socket Socket { get; }
    }

    public class SocketService {
        private readonly AppData _appData;

        public SocketService(AppData appData) {
            _appData = appData;
        }

        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public void StartListening() {
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                var ipAddress = ipHostInfo.AddressList[0];
                var localEndPoint = new IPEndPoint(ipAddress, TcpInfo.Port);

                var listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try {
                    listener.Bind(localEndPoint);
                    listener.Listen(100);

                    while (!_appData.ShuttingDown) {
                        allDone.Reset();

                        Console.WriteLine("Listening for TCP commands . . .");
                        listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                        allDone.WaitOne();
                    }

                } catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }

            };
            worker.RunWorkerAsync();
        }

        public void AcceptCallback(IAsyncResult ar) {
            allDone.Set();

            // Get the socket that handles the client request.  
            var listener = (Socket)ar.AsyncState;
            var handler = listener.EndAccept(ar);

            // Create the state object.  
            var state = new StateObject {
                Socket = handler,
                Service = this
            };
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar) {
            var state = (StateObject)ar.AsyncState;
            var socket = state.Socket;
            var service = state.Service;

            // Read data from the client socket.
            int bytesRead = socket.EndReceive(ar);
            if (bytesRead > 0) {
                // There  might be more data, so store the data received so far.  
                state.Data.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

                var payload = state.Data.ToString();
                if (payload.IndexOf("<EOF>") > -1) {
                    service.HandleRequest(new TcpRequest(payload, socket));
                } else {
                    socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private void HandleRequest(TcpRequest request) {
            Console.WriteLine("Handling Rquest: " + request.RequestType.AsString());
            if (request.RequestType == AoTcpRequest.GetCardInfo) {
                HandleGetCardInfo(request);
            }
        }

        private void HandleGetCardInfo(TcpRequest request) {
            var getCardInfoRequest = JsonConvert.DeserializeObject<GetCardInfoRequest>(request.Body);
            var cardIndex = getCardInfoRequest.Index + 1;

            var cards = _appData.Game.Players[0].SelectableCards.CardButtons;

            var card = (cardIndex < cards.Count) ? cards[cardIndex] as Card : null;
            var getCardInfoReponse = (card == null)
                ? new GetCardInfoReponse { CardType = "None", Name = "" }
                : new GetCardInfoReponse { CardType = card.Faction, Name = card.Name };

            Send(request.Socket, getCardInfoReponse.ToString());
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
