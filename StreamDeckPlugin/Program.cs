using ArkhamOverlay.TcpUtils;
using StreamDeckPlugin.Utils;

namespace StreamDeckPlugin {
    class Program {
        public static void Main(string[] args) {
#if DEBUG
            // optional, but recommended
//            System.Diagnostics.Debugger.Launch();
#endif

            new ReceiveSocketService(new TcpRequestHandler()).StartListening(StreamDeckTcpInfo.Port);

            // register actions and connect to the Stream Deck
            SharpDeck.StreamDeckPlugin.Run();
        }
    }
}
