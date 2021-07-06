using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.Interfaces;
using EideticMemoryOverlay.PluginApi.LocalCards;
using Emo.Data;
using Emo.Services;
using Emo.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using Newtonsoft.Json;
using PageController;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Emo.Pages.LocalImages {
    public class LocalImagesController<T> : Controller<LocalImagesView, LocalImagesViewModel>, IDisplayableView where T : LocalCard, new() {
        private readonly IPlugIn _plugIn;
        private readonly LoggingService _logger;
        private readonly ILocalCardEditor _localCardEditor;

        public LocalImagesController(IPlugIn plugIn, LoggingService logger, ILocalCardsService<T> localCardsService) {
            _plugIn = plugIn;
            _logger = logger;
            ViewModel.LocalImagesDirectory = _plugIn.LocalImagesDirectory;

            LoadPacks();

            _localCardEditor = _plugIn.CreateLocalCardEditor();
            View.CustomEditor.Content = _localCardEditor;

            View.Closed += (s, e) => {
                localCardsService.InvalidateManifestCache();
                plugIn.LoadEncounterCards();
            };
        }

        public void ShowView() {
            View.ShowDialog();
        }

        private void LoadPacks() {
            if (!Directory.Exists(_plugIn.LocalImagesDirectory)) {
                _logger.LogMessage($"Directory '{_plugIn.LocalImagesDirectory}' not found.");
                return;
            }

            var packs = new List<LocalPack>();
            try {
                _logger.LogMessage($"Loading packs from {_plugIn.LocalImagesDirectory}.");
                foreach (var directory in Directory.GetDirectories(_plugIn.LocalImagesDirectory)) {
                    packs.Add(LoadPack(directory));                    
                }
                ViewModel.Packs = packs;
                ViewModel.AnyPacks = packs.Any();
            } catch (Exception e) {
                _logger.LogException(e, "Error loading packs");
            }
        }

        private LocalPack LoadPack(string directory) {
            var pack = new LocalPack(directory);
            pack.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(LocalPack.SelectedCard)) {
                    _localCardEditor.SetCard(pack.SelectedCard);
                    return;
                }
            };

            var manifestPath = directory + "\\Manifest.json";
            if (File.Exists(manifestPath)) {
                try {
                    _logger.LogMessage($"Loading pack manifest {manifestPath}.");
                    var manifest = JsonConvert.DeserializeObject<LocalPackManifest<T>>(File.ReadAllText(manifestPath));
                    foreach (var card in manifest.Cards) {
                        card.FilePath = Path.GetDirectoryName(manifestPath) + "\\" + card.FileName;
                    }
                    ReadManifest(manifest, pack);

                    pack.PropertyChanged += (s, e) => {
                        if (e.PropertyName == nameof(LocalPack.SelectedCard) || e.PropertyName == nameof(LocalPack.IsCardSelected)) {
                            return;
                        }

                        WriteManifest();
                    };
                } catch (Exception e) {
                    _logger.LogException(e, "Error loading pack manifest");
                }
            }

            return pack;
        }

        private EditableLocalCard LoadCard(LocalPack pack, string filePath) {
            if (string.Equals(Path.GetExtension(filePath), ".json", StringComparison.InvariantCulture)) {
                return null;
            }

            //the backs of cards will be loaded with the front- adding -back to a file denotes it's the back of a card
            if (Path.GetFileNameWithoutExtension(filePath).ToLower().EndsWith("-back")) {
                return null;
            }
            
            try {
                var card = pack.Cards.FirstOrDefault(x => string.Equals(x.FilePath, filePath, StringComparison.InvariantCulture));
                if (card == null) {
                    card = _plugIn.CreateEditableLocalCard();
                       
                    card.FilePath = filePath;
                    card.PropertyChanged += (s, e) => {
                        WriteManifest();
                    };
                    pack.Cards.Add(card);
                }

                LoadCardImages(card);

                return card;
            } catch {
                //this isn't an image- that's completely valid, we just ignore it and move on
                return null;
            }
        }

        private void LoadCardImages(EditableLocalCard card) {
            if (card.Image != null) {
                return;
            }

            card.FrontThumbnail = ShellFile.FromFilePath(card.FilePath).Thumbnail.BitmapSource;

            card.Image = ImageUtils.LoadLocalImage(new Uri(card.FilePath, UriKind.Absolute));

            var cardBackPath = Path.GetDirectoryName(card.FilePath) + "\\" + Path.GetFileNameWithoutExtension(card.FilePath) + "-back" + Path.GetExtension(card.FilePath);
            if (File.Exists(cardBackPath)) {
                card.BackThumbnail = ShellFile.FromFilePath(cardBackPath).Thumbnail.BitmapSource;
                card.HasBack = true;
            }
        }

        internal void ReadManifest(LocalPackManifest<T> manifest, LocalPack pack) {
            pack.Name = manifest.Name;
            var localCards = new ObservableCollection<EditableLocalCard>();
            foreach (var card in manifest.Cards) {
                if (File.Exists(card.FilePath)) {
                    var editableLocalCard = _plugIn.CreateEditableLocalCard();
                    card.CopyTo(editableLocalCard);

                    editableLocalCard.PropertyChanged += (s, e) => {
                        WriteManifest();
                    };

                    localCards.Add(editableLocalCard);
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
                var manifest = new LocalPackManifest {
                    Name = pack.Name,
                    Cards = new List<LocalCard>()
                };

                foreach (var editableLocalCard in pack.Cards) {
                    var localCard = new T();
                    localCard.CopyFrom(editableLocalCard);
                    manifest.Cards.Add(localCard);
                }

                File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest));
            } catch (Exception e) {
                _logger.LogException(e, "Error writing manifest");
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
                _logger.LogException(e, "Error loading images");
            }
        }

        [Command]
        public void SelectDirectory() {
            _logger.LogMessage("LocalImages window: select local images directory clicked.");
            var dialog = new CommonOpenFileDialog {
                InitialDirectory = _plugIn.LocalImagesDirectory,
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                _logger.LogMessage($"LocalImages: new directory: {dialog.FileName}.");
                _plugIn.LocalImagesDirectory = dialog.FileName;
                ViewModel.LocalImagesDirectory = _plugIn.LocalImagesDirectory;
                LoadPacks();
            }
            View.Activate();
        }
    }
}
