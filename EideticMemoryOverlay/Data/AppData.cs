using PageController;

namespace Emo.Data {
    public class AppData : ViewModel {
        public AppData(Configuration configuration, Game game) {
            Configuration = configuration;
            Game = game;
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
