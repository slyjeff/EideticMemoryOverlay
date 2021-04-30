using EideticMemoryOverlay.PluginApi;
using Emo.Data;
using Emo.Services;
using PageController;

namespace Emo.Pages.Main {
    public class MainViewModel : ViewModel {
        public virtual AppData AppData { get; set; }

        public virtual IGameData Game { get { return AppData.Game;  } }

        public virtual Configuration Configuration { get { return AppData.Configuration; } }

        public virtual LoadingStatusService LoadingStatus { get; set; }

        public virtual bool OverlayDisplayed { get; set; }
    }
}
