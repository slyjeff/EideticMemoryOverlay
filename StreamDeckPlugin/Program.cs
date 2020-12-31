namespace ArkahmOverlayStreamDeck {
    class Program {
        public static void Main(string[] args) {
#if DEBUG
            // optional, but recommended
//            System.Diagnostics.Debugger.Launch();
#endif

            // register actions and connect to the Stream Deck
            SharpDeck.StreamDeckPlugin.Run();
        }
    }
}
