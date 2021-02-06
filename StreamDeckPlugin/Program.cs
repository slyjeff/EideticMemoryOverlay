using ArkhamOverlay.Common.Tcp;
using StreamDeckPlugin.Services;
using StreamDeckPlugin.Utils;

namespace StreamDeckPlugin {
    class Program {
        public static void Main(string[] args) {
#if DEBUG
            // optional, but recommended
            System.Diagnostics.Debugger.Launch();
#endif
            var container = new StructureMap.Container(x => {
                x.Scan(y => {
                    y.TheCallingAssembly();
                    y.WithDefaultConventions();
                });
            });

            container.Configure(x => {
                x.For<IEventBus>().Use<EventBus>().Singleton();
                x.For<IDynamicActionInfoStore>().Use<DynamicActionInfoStore>().Singleton();
                x.For<ISendEventHandler>().Use<SendEventHandler>().Singleton();
                x.For<IImageService>().Use<ImageService>().Singleton();
                x.For<IRequestHandler>().Use<TcpRequestHandler>();
                x.For<IReceiveSocketService>().Use<ReceiveSocketService>();
                x.For<IEstablishConnectionToUiService>().Use<EstablishConnectionToUiService>();
            });

            ServiceLocator.Container = container;

            //keep references or garbage collection will clean this up and we'll stop receiving events
            var sendEventHandler = container.GetInstance<ISendEventHandler>();
            var receiveSocketService = container.GetInstance<IReceiveSocketService>();
            receiveSocketService.StartListening(StreamDeckTcpInfo.Port);

            var establishConnectionToUiService = container.GetInstance<IEstablishConnectionToUiService>();
            establishConnectionToUiService.AttemptToEstablishConnection();

            // register actions and connect to the Stream Deck
            SharpDeck.StreamDeckPlugin.Run();
        }
    }
}
