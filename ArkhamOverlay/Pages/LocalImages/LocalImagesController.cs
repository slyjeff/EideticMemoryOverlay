﻿using ArkhamOverlay.Data;
using ArkhamOverlay.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using Newtonsoft.Json;
using PageController;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

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
                _logger.LogError(e.Message);
            }
        }

        private LocalPack LoadPack(string directory) {
            var pack = new LocalPack(directory);

            var manifestPath = directory + "\\Manifest.json";
            if (File.Exists(manifestPath)) {
                try {
                    _logger.LogMessage($"Loading pack manifest {manifestPath}.");
                    var manifest = JsonConvert.DeserializeObject<PackManifest>(File.ReadAllText(manifestPath));
                    ReadManifest(manifest, pack);

                    pack.PropertyChanged += (s, e) => {
                        if (e.PropertyName == nameof(LocalPack.SelectedCard) || e.PropertyName == nameof(LocalPack.IsCardSelected)) {
                            return;
                        }

                        WriteManifest();
                    };
                } catch (Exception e) {
                    _logger.LogError(e.Message);
                }
            }

            return pack;
        }

        private LocalCard LoadCard(LocalPack pack, string filePath) {
            if (string.Compare(Path.GetExtension(filePath), ".json", StringComparison.InvariantCulture) == 0) {
                return null;
            }

            //the backs of cards will be loaded with the front- adding -back to a file denotes it's the back of a card
            if (Path.GetFileNameWithoutExtension(filePath).ToLower().EndsWith("-back")) {
                return null;
            }
            
            try {
                var card = pack.Cards.FirstOrDefault(x => string.Compare(x.FilePath, filePath, StringComparison.InvariantCulture) == 0);
                if (card == null) {
                    card = new LocalCard(filePath);
                    card.PropertyChanged += (s, e) => {
                        WriteManifest();
                    };
                }

                LoadCardImages(card);

                return card;
            } catch {
                //this isn't an image- that's completely valid, we just ignore it and move on
                return null;
            }
        }

        private void LoadCardImages(LocalCard card) {
            if (card.Image != null) {
                return;
            }

            card.FrontThumbnail = ShellFile.FromFilePath(card.FilePath).Thumbnail.BitmapSource;

            var isHorizontal = card.FrontThumbnail.Width > card.FrontThumbnail.Width;

            var image = new BitmapImage();
            image.BeginInit();

            // Set properties.
            image.CacheOption = BitmapCacheOption.OnDemand;
            image.CreateOptions = BitmapCreateOptions.DelayCreation;
            if (isHorizontal) {
                image.DecodePixelWidth = 400;
            } else {
                image.DecodePixelHeight = 400;
            }
            image.UriSource = new Uri(card.FilePath, UriKind.Absolute);
            image.EndInit();
            card.Image = image;

            var cardBackPath = Path.GetDirectoryName(card.FilePath) + "\\" + Path.GetFileNameWithoutExtension(card.FilePath) + "-back" + Path.GetExtension(card.FilePath);
            if (File.Exists(cardBackPath)) {
                card.BackThumbnail = ShellFile.FromFilePath(cardBackPath).Thumbnail.BitmapSource;
                card.HasBack = true;
            }
        }

        internal void ReadManifest(PackManifest manifest, LocalPack pack) {
            pack.Name = manifest.Name;
            var localCards = new List<LocalCard>();
            foreach (var card in manifest.Cards) {
                if (File.Exists(card.FilePath)) {
                    var localCard = new LocalCard(card.FilePath) {
                        CardType = card.CardType,
                        Name = card.Name,
                        HasBack = card.HasBack
                    };

                    localCard.PropertyChanged += (s, e) => {
                        WriteManifest();
                    };

                    localCards.Add(localCard);
                }
            }
            pack.Cards = localCards;
        }

        public void WriteManifest() {
            var pack = ViewModel.SelectedPack;
            if (pack == null) {
                _logger.LogError("Attempting to write manifest with no pack select");
                return;
            }

            var manifestPath = pack.Directory + "\\Manifest.json";
            _logger.LogMessage($"Writing manifest {manifestPath}.");
            try {
                var manifest = new PackManifest(pack);
                File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest));
            } catch (Exception e) {
                _logger.LogError(e.Message);
            }
        }

        [PropertyChanged]
        public void SelectedPackChanged() {
            var pack = ViewModel.SelectedPack;
            if (pack == null) {
                return;
            }

            _logger.LogMessage($"Loading images from directory {pack.Directory}.");
            try {
                foreach (var file in Directory.GetFiles(pack.Directory)) {
                    LoadCard(pack, file); 
                }
            } catch (Exception e) {
                _logger.LogError(e.Message);
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