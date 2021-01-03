using ArkhamOverlay.Data;
using ArkhamOverlay.Pages.Main;
using ArkhamOverlay.Services;
using ArkhamOverlay.TcpUtils;
using PageController;
using System.Windows;

namespace ArkhamOverlay {
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            var container = new StructureMap.Container(x => {
                x.Scan(y => {
                    y.TheCallingAssembly();
                    y.WithDefaultConventions();
                });
            });
            
            PageControllerConfiguration.PageDependencyResolver = new StructureMapDependencyResolver(container);

            container.Configure(x => {
                x.For<IRequestHandler>().Use<TcpRequestHandler>();
                x.For<AppData>().Use(new AppData());
                x.For<IControllerFactory>().Use(new ControllerFactory(container));
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
    }
}
