using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.Interfaces;
using EideticMemoryOverlay.PluginApi.LocalCards;
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
                x.For(typeof(ILocalCardsService<>)).Use(typeof(LocalCardsService<>));
                x.For<AppData>().Use<AppData>().Singleton();
                x.For<IAppData>().Use<AppData>();
                x.For<Configuration>().Use<Configuration>().Singleton();
                x.For<IPlugIn>().Use<PlugInWrapper>().Singleton();
                x.For<IPlugInService>().Use<PlugInService>().Singleton();
                x.For<IGameData>().Use<Game>().Singleton();
                x.For<IGameFileService>().Use<GameFileService>().Singleton();
                x.For<IControllerFactory>().Use(new ControllerFactory(container));
                x.For<ICardGroup>().Use<CardGroup>();
            });
           
            _loggingService = container.GetInstance<LoggingService>();

            var configurationService = container.GetInstance<ConfigurationService>();
            configurationService.Load();

            var gameFileService = container.GetInstance<IGameFileService>();
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
