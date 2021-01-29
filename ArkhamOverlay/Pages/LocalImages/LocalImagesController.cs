using ArkhamOverlay.Data;
using ArkhamOverlay.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
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
            pack.PropertyChanged += (s, e) => {
                PackChanged(pack);
            };

            var manifestPath = directory + "\\Manifest.json";
            if (File.Exists(manifestPath)) {
                try {
                    _logger.LogMessage($"Loading pack manifest {manifestPath}.");
                    var data = JsonConvert.DeserializeObject<PackManifest>(File.ReadAllText(manifestPath));
                    pack.Name = data.Name;
                } catch (Exception e) {
                    _logger.LogMessage(e.Message);
                }
            }

            return pack;
        }

        private void PackChanged(LocalPack pack) {
            var manifestPath = pack.Directory + "\\Manifest.json";
            _logger.LogMessage($"saving pack manifest {manifestPath}.");

            try {
                var manifest = new PackManifest(pack);
                File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest));
            } catch (Exception e) {
                _logger.LogMessage(e.Message);
            }
        }

        private LocalCard LoadCard(string filePath) {
            if (string.Compare(Path.GetExtension(filePath), ".json", StringComparison.InvariantCulture) == 0) {
                return null;
            }

            //the backs of cards will be loaded with the front- adding -back to a file denotes it's the back of a card
            if (Path.GetFileNameWithoutExtension(filePath).ToLower().EndsWith("-back")) {
                return null;
            }
            
            try {
                var card = new LocalCard(filePath);
                card.FrontImage = ShellFile.FromFilePath(filePath).Thumbnail.BitmapSource;
                var cardBackPath = Path.GetDirectoryName(filePath) + "\\" + Path.GetFileNameWithoutExtension(filePath) + "-back" + Path.GetExtension(filePath);
                if (File.Exists(cardBackPath)) {
                    card.BackImage = ShellFile.FromFilePath(cardBackPath).Thumbnail.BitmapSource;
                }

                return card;
            } catch {
                //this isn't an image- that's completely valid, we just ignore it and move on
                return null;
            }
        }


        [PropertyChanged]
        public void SelectedPackChanged() {
            var pack = ViewModel.SelectedPack;

            ViewModel.IsPackSelected = pack != null;
            if (pack == null) {
                return;
            }

            _logger.LogMessage($"Loading images from directory {pack.Directory}.");
            try {
                var cards = new List<LocalCard>();
                foreach (var file in Directory.GetFiles(pack.Directory)) {
                    var card = LoadCard(file); 
                    if (card != null) {
                        cards.Add(card);
                    }
                }
                pack.Cards = cards;
            } catch (Exception e) {
                _logger.LogMessage(e.Message);
            }
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
