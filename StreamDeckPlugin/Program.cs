using ArkhamOverlay.TcpUtils;
using StreamDeckPlugin.Utils;
using System.Threading;
using System.Timers;

namespace StreamDeckPlugin {
    class Program {
        public static void Main(string[] args) {
#if DEBUG
            // optional, but recommended
            //System.Diagnostics.Debugger.Launch();
#endif

            var requestHandler = new TcpRequestHandler();

            //keep references or garbage collection will clean this up and we'll stop receiving events
            var receiveSocketService = new ReceiveSocketService(requestHandler);
            receiveSocketService.StartListening(StreamDeckTcpInfo.Port);

            var registerForUpdatesService = new RegisterForUpdatesService(requestHandler);
            registerForUpdatesService.RegisterForUpdates();

            // register actions and connect to the Stream Deck
            SharpDeck.StreamDeckPlugin.Run();
        }
    }
}
