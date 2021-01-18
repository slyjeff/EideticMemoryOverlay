using PageController;

namespace ArkhamOverlay.Data {
    public class AppData : ViewModel {
        public AppData() {
            Configuration = new Configuration {
                OverlayWidth = 1228,
                OverlayHeight = 720,
                CardHeight = 300
            };
            Game = new Game(Configuration);
        }

        public Game Game { get; }

        public Configuration Configuration { get; }

        private string _status;

        public string Status {
            get => _status; 
            set {
                _status = value;
                NotifyPropertyChanged(nameof(Status));
            }
        }

        public void OnGameChanged() {
            NotifyPropertyChanged(nameof(Game));
        }
    }
}
