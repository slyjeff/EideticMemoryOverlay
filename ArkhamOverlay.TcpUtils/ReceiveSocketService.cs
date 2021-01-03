using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ArkhamOverlay.TcpUtils {
    public interface IRequestHandler {
        void HandleRequest(TcpRequest tcpRequest);
    }

    internal class StateObject {
        public const int BufferSize = 1024;

        public byte[] Buffer = new byte[BufferSize];

        public StringBuilder Data = new StringBuilder();

        public Socket Socket = null;

        public IRequestHandler RequestHandler;
    }

    public class TcpRequest {
        public TcpRequest(string request, Socket socket) {
            var endOfCode = request.IndexOf(":") - 1;
            var startOfBody = endOfCode + 2;
            var endOfBody = request.IndexOf("<EOF>") - 1;
            var bodyLength = endOfBody - startOfBody + 1;

            var code = request.Substring(0, endOfCode + 1);
            RequestType = code.AsAoTcpRequest();
            Body = request.Substring(startOfBody, bodyLength);
            Socket = socket;
        }

        public AoTcpRequest RequestType { get; }
        public string Body { get; }
        public Socket Socket { get; }
    }

    public class ReceiveSocketService {
        private readonly IRequestHandler _requestHandler;

        public ReceiveSocketService(IRequestHandler requestHandler) {
            _requestHandler = requestHandler;
        }

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public void StartListening(int port) {
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                var ipAddress = ipHostInfo.AddressList[0];
                var localEndPoint = new IPEndPoint(ipAddress, port);

                var listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try {
                    listener.Bind(localEndPoint);
                    listener.Listen(100);

                    while (true) {
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

            var listener = (Socket)ar.AsyncState;
            var handler = listener.EndAccept(ar);

            var state = new StateObject {
                Socket = handler,
                RequestHandler = _requestHandler
            };
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar) {
            var state = (StateObject)ar.AsyncState;
            var socket = state.Socket;
            var requestHandler = state.RequestHandler;

            int bytesRead = socket.EndReceive(ar);
            if (bytesRead > 0) {
                // There  might be more data, so store the data received so far.  
                state.Data.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

                var payload = state.Data.ToString();
                if (payload.IndexOf("<EOF>") > -1) {
                    requestHandler.HandleRequest(new TcpRequest(payload, socket));
                } else {
                    socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
        }
    }
}
