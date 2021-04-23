using EideticMemoryOverlay.PluginApi;
using Emo.Common.Services;
using Emo.Common.Tcp;
using Emo.Common.Utils;
using Emo.Data;
using Emo.Pages.Main;
using Emo.Services;
using PageController;
using System.Windows;

namespace Emo {
    public partial class App : Application
    {
        private LoggingService _loggingService;

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            var container = new StructureMap.Container(x => {
                x.Scan(y => {
                    y.TheCallingAssembly();
                    y.WithDefaultConventions();
                });
            });

            ServiceLocator.Container = container;

            PageControllerConfiguration.PageDependencyResolver = new StructureMapDependencyResolver(container);

            var eventBus = new UiEventBus();
            container.Configure(x => {
                x.For<LoggingService>().Use<LoggingService>().Singleton();
                x.For<ILoggingService>().Use<LoggingService>();
                x.For<IEventBus>().Use(eventBus);
                x.For<ICrossAppEventBus>().Use(eventBus);
                x.For<IBroadcastService>().Use<BroadcastService>().Singleton();
                x.For<IRequestHandler>().Use<TcpRequestHandler>();
                x.For<ICardInfoButtonFactory>().Use<CardInfoButtonFactory>().Singleton();
                x.For<AppData>().Use<AppData>().Singleton();
                x.For<Configuration>().Use<Configuration>().Singleton();
                x.For<IPlugInService>().Use<PlugInService>();
                x.For<Game>().Use<Game>().Singleton();
                x.For<IControllerFactory>().Use(new ControllerFactory(container));
            });
           
            _loggingService = container.GetInstance<LoggingService>();

            var cardLoadService = container.GetInstance<CardLoadService>();
            cardLoadService.RegisterEvents();

            var configurationService = container.GetInstance<ConfigurationService>();
            configurationService.Load();

            var gameFileService = container.GetInstance<GameFileService>();
            gameFileService.Load();

            var receiveSocketService = container.GetInstance<ReceiveSocketService>();
            receiveSocketService.StartListening(TcpInfo.EmoPort);

            var controller = container.GetInstance<MainController>();
            controller.View.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            if(_loggingService != null) {
                _loggingService.LogException(e.Exception, "Unhandled exception occured.");
            }

            var exceptionMessage = e.Exception.InnerException == null ? e.Exception.Message : e.Exception.InnerException.Message;
            MessageBox.Show($"An unhandled exception just occurred: {exceptionMessage}", "Eidetic Memory Overlay", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }
    }
}
