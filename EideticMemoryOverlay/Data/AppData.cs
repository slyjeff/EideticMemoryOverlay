using EideticMemoryOverlay.PluginApi.Interfaces;
using PageController;

namespace Emo.Data {
    public class AppData : ViewModel, IAppData {
        public AppData(Configuration configuration, IGameData game) {
            Configuration = configuration;
            Game = game;
        }

        public IGameData Game { get; }

        public Configuration Configuration { get; }

        private string _status;

        public string Status {
            get => _status; 
            set {
                _status = value;
                NotifyPropertyChanged(nameof(Status));
            }
        }
    }
}
