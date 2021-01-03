using ArkhamOverlay.Data;
using PageController;

namespace ArkhamOverlay.Pages.Main {
    public class MainViewModel : ViewModel {
        public virtual AppData AppData { get; set; }

        public virtual Game Game { get { return AppData.Game;  } }
        public virtual Configuration Configuration { get { return AppData.Configuration; } }
    }
}
