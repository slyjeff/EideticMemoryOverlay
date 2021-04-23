using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EideticMemoryOverlay.PluginApi {
    public class LocalCardsService {
        private static IList<LocalPackManifest> _cachedManifests = null;

        private readonly AppData _appData;
        private readonly ILoggingService _logger;

        public LocalCardsService(ILoggingService logger, AppData appData) {
            _appData = appData;
            _logger = logger;
        }

        public List<LocalCard> LoadLocalCards() {
            var cards = new List<LocalCard>();

            foreach (var manifest in GetLocalPackManifests()) {
                _logger.LogMessage($"Loading Local Cards from {manifest.Name}.");
                cards.AddRange(manifest.Cards);
            }

            return LoadLocalCardsImpl(null);
        }

        public List<LocalCard> LoadLocalCardsFromPacks(IList<string> packsToLoad) {
            return LoadLocalCardsImpl(packsToLoad);
        }

        public List<LocalCard> LoadLocalCardsImpl(IList<string> packsToLoad) {
            var cards = new List<LocalCard>();

            foreach (var manifest in GetLocalPackManifests()) {
                if (packsToLoad != null && !packsToLoad.Any(x => string.Equals(x, manifest.Name, StringComparison.InvariantCulture))) {
                    continue;
                }

                _logger.LogMessage($"Loading Local Cards from {manifest.Name}.");
                cards.AddRange(manifest.Cards);
            }

            return cards;
        }

        public IList<LocalPackManifest> GetLocalPackManifests() {
            if (_cachedManifests != null) {
                return _cachedManifests;
            }

            var manifests = new List<LocalPackManifest>();

            if (!Directory.Exists(_appData.Configuration.LocalImagesDirectory)) {
                return manifests;
            }

            _logger.LogMessage("Loading local pack manifests");
            try {
                foreach (var directory in Directory.GetDirectories(_appData.Configuration.LocalImagesDirectory)) {
                    var manifestPath = directory + "\\Manifest.json";
                    if (File.Exists(manifestPath)) {
                        manifests.Add(JsonConvert.DeserializeObject<LocalPackManifest>(File.ReadAllText(manifestPath)));
                    }
                }
                _cachedManifests = manifests;
            } catch (Exception e) {
                _logger.LogException(e, "Error loading local pack manifests");
            }

            return manifests;
        }

        public LocalCard GetCardById(string ArkhamDbId) {
            if (string.IsNullOrEmpty(ArkhamDbId)) {
                return null;
            }

            foreach (var manifest in GetLocalPackManifests()) {
                foreach (var card in manifest.Cards) {
                    if (card.ArkhamDbId == ArkhamDbId) {
                        return card;
                    }
                }
            }

            return null;
        }

        public void InvalidateManifestCache() {
            _cachedManifests = null;
        }
    }
}
