using ArkhamOverlay.TcpUtils;
using StreamDeckPlugin.Services;
using StreamDeckPlugin.Utils;

namespace StreamDeckPlugin {
    class Program {
        public static void Main(string[] args) {
#if DEBUG
            // optional, but recommended
            //System.Diagnostics.Debugger.Launch();
#endif
            var container = new StructureMap.Container(x => {
                x.Scan(y => {
                    y.TheCallingAssembly();
                    y.WithDefaultConventions();
                });
            });

            container.Configure(x => {
                x.For<IDynamicActionInfoService>().Use<DynamicActionInfoService>().Singleton();
                x.For<ISendSocketService>().Use<StreamDeckSendSocketService>().Singleton();
                x.For<IImageService>().Use<ImageService>().Singleton();
                x.For<IRequestHandler>().Use<TcpRequestHandler>();
                x.For<IReceiveSocketService>().Use<ReceiveSocketService>();
                x.For<IRegisterForUpdatesService>().Use<RegisterForUpdatesService>();
            });

            ServiceLocator.Container = container;

            //keep references or garbage collection will clean this up and we'll stop receiving events
            var receiveSocketService = container.GetInstance<IReceiveSocketService>();
            receiveSocketService.StartListening(StreamDeckTcpInfo.Port);

            var registerForUpdatesService = container.GetInstance<IRegisterForUpdatesService>();
            registerForUpdatesService.RegisterForUpdates();

            // register actions and connect to the Stream Deck
            SharpDeck.StreamDeckPlugin.Run();
        }
    }
}
