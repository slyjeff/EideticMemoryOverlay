using ArkhamOverlay.Data;
using ArkhamOverlay.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using PageController;
using System;
using System.Collections.Generic;
using System.IO;

namespace ArkhamOverlay.Pages.LocalImages {
    public class LocalImagesController : Controller<LocalImagesView, LocalImagesViewModel> {
        private readonly AppData _appData;
        private readonly LoggingService _logger;

        public LocalImagesController(AppData appData, LoggingService logger) {
            _appData = appData;
            _logger = logger;
            ViewModel.Configuration = appData.Configuration;

            LoadPacks();
        }

        internal void ShowView() {
            View.ShowDialog();
        }
        private void LoadPacks() {
            if (!Directory.Exists(_appData.Configuration.LocalImagesDirectory)) {
                _logger.LogMessage($"Directory '{_appData.Configuration.LocalImagesDirectory}' not found.");

            }

            var packs = new List<LocalPack>();
            try {
                _logger.LogMessage($"Loading packs from {_appData.Configuration.LocalImagesDirectory}.");
                foreach (var directory in Directory.GetDirectories(_appData.Configuration.LocalImagesDirectory)) {
                    packs.Add(LoadPack(directory));                    
                }
                ViewModel.Packs = packs;
            } catch (Exception e) {
                _logger.LogMessage(e.Message);
            }
        }

        private LocalPack LoadPack(string directory) {
            var pack = new LocalPack(directory);

            var manifestPath = directory + "Manifest.json";
            if (File.Exists(manifestPath)) {
                try {
                    _logger.LogMessage($"Loading pack manifest {manifestPath}.");
                    var data = JsonConvert.DeserializeObject<PackManifest>(File.ReadAllText(directory + "Manifest.json"));
                    pack.Name = data.Name;
                } catch (Exception e) {
                    _logger.LogMessage(e.Message);
                }
            }

            return pack;
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
                LoadPacks();
            }
            View.Activate();
        }
    }
}
