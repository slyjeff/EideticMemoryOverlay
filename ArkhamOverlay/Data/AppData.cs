using PageController;

namespace ArkhamOverlay.Data {
    public class AppData : ViewModel {
        public AppData() {
            Game = new Game();
            Configuration = new Configuration {
                OverlayWidth = 1228,
                OverlayHeight = 720,
                CardHeight = 300
            };
        }

        public Game Game { get; }
        public Configuration Configuration { get; }
        public bool ShuttingDown { get; set; }

        public void OnGameChanged() {
            NotifyPropertyChanged(nameof(Game));
        }
    }
}
