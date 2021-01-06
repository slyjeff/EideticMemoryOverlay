using ArkhamOverlay.Data;
using ArkhamOverlay.Services;
using PageController;

namespace ArkhamOverlay.Pages.Main {
    public class MainViewModel : ViewModel {
        public virtual AppData AppData { get; set; }

        public virtual Game Game { get { return AppData.Game;  } }

        public virtual Configuration Configuration { get { return AppData.Configuration; } }

        public virtual LoadingStatusService LoadingStatus { get; set; }
    }
}
