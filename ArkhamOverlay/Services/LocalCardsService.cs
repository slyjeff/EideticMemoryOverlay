using ArkhamOverlay.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArkhamOverlay.Services {
    public class LocalCardsService {
        private static IList<LocalPackManifest> _cachedManifests = null;

        private readonly AppData _appData;
        private readonly LoggingService _logger;

        public LocalCardsService(LoggingService logger, AppData appData) {
            _appData = appData;
            _logger = logger;
        }

        public List<Card> LoadEncounterCards() {
            var cards = new List<Card>();

            foreach (var manifest in GetLocalPackManifests()) {
                if (!_appData.Game.LocalPacks.Any(x => string.Equals(x, manifest.Name, StringComparison.InvariantCulture))) {
                    continue;
                }

                _logger.LogMessage($"Loading Local Cards from {manifest.Name}.");
                foreach (var card in manifest.Cards) {
                    try {
                        cards.Add(new Card(card, false));
                        if (card.HasBack) {
                            cards.Add(new Card(card, true));
                        }
                    } catch (Exception e) {
                        _logger.LogException(e, "Error loading local cards");
                    }
                }
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

        internal LocalManifestCard GetCardById(string ArkhamDbId) {
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
