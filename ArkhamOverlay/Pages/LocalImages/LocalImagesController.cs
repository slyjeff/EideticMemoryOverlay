using ArkhamOverlay.Data;
using PageController;
using System;

namespace ArkhamOverlay.Pages.LocalImages {
    public class LocalImagesController : Controller<LocalImagesView, LocalImagesViewModel> {
        private readonly AppData _appData;

        public LocalImagesController(AppData appData) {
            _appData = appData;
        }

        internal void ShowView() {
            View.ShowDialog();
        }
    }
}
