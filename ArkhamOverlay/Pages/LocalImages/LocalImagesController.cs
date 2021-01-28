using ArkhamOverlay.Data;
using ArkhamOverlay.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using PageController;

namespace ArkhamOverlay.Pages.LocalImages {
    public class LocalImagesController : Controller<LocalImagesView, LocalImagesViewModel> {
        private readonly AppData _appData;
        private readonly LoggingService _logger;

        public LocalImagesController(AppData appData, LoggingService logger) {
            _appData = appData;
            _logger = logger;
            ViewModel.Configuration = appData.Configuration;
        }

        internal void ShowView() {
            View.ShowDialog();
        }

        [Command]
        public void SelectDirectory() {
            _logger.LogMessage("LocalImages window: select local images directory clicked.");
            var dialog = new CommonOpenFileDialog {
                InitialDirectory = _appData.Configuration.LocalImagesDirectory,
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                _logger.LogMessage($"LocalImages: new directory: {dialog.FileName}.");
                _appData.Configuration.LocalImagesDirectory = dialog.FileName;
            }
        }
    }
}
