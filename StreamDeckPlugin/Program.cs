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
            new ReceiveSocketService(requestHandler).StartListening(StreamDeckTcpInfo.Port);
            new RegisterForUpdatesService(requestHandler).RegisterForUpdates();

            // register actions and connect to the Stream Deck
            SharpDeck.StreamDeckPlugin.Run();
        }
    }
}
