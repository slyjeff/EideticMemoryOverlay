using ArkhamOverlay.Data;
using ArkhamOverlay.Pages.Main;
using ArkhamOverlay.Services;
using ArkhamOverlay.TcpUtils;
using ArkhamOverlay.Utils;
using PageController;
using System.Windows;

namespace ArkhamOverlay {
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

            _loggingService = new LoggingService();
            var eventService = new ActionService();

            container.Configure(x => {
                x.For<IRequestHandler>().Use<TcpRequestHandler>();
                x.For<AppData>().Use(new AppData());
                x.For<IControllerFactory>().Use(new ControllerFactory(container));
                x.For<LoggingService>().Use(_loggingService);
                x.For<IActionRequestService>().Use(eventService);
                x.For<IActionNotificationService>().Use(eventService);
            });

            var cardLoadService = container.GetInstance<CardLoadService>();
            cardLoadService.RegisterEvents();

            var configurationService = container.GetInstance<ConfigurationService>();
            configurationService.Load();

            var gameFileService = container.GetInstance<GameFileService>();
            gameFileService.Load("LastSaved.json");

            var receiveSocketService = container.GetInstance<ReceiveSocketService>();
            receiveSocketService.StartListening(TcpInfo.ArkhamOverlayPort);

            var controller = container.GetInstance<MainController>();
            controller.View.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            if(_loggingService != null) {
                _loggingService.LogException(e.Exception, "Unhandled exception occured.");
            }

            var exceptionMessage = e.Exception.InnerException == null ? e.Exception.Message : e.Exception.InnerException.Message;
            MessageBox.Show($"An unhandled exception just occurred: {exceptionMessage}", "Arkham Overlay", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }
    }
}
