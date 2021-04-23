using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EideticMemoryOverlay.PluginApi.LocalCards {
    public interface ILocalCardsService<T> where T : LocalCard {
        List<T> LoadLocalCards();
        List<T> LoadLocalCardsFromPacks(IList<string> packsToLoad);
        IEnumerable<T> AllCards();
        void InvalidateManifestCache();
        IList<LocalPackManifest<T>> GetLocalPackManifests();
    }

    public class LocalCardsService<T> : ILocalCardsService<T> where T : LocalCard {
        private static IList<LocalPackManifest<T>> _cachedManifests = null;

        private readonly AppData _appData;
        private readonly ILoggingService _logger;

        public LocalCardsService(ILoggingService logger, AppData appData) {
            _appData = appData;
            _logger = logger;
        }

        public List<T> LoadLocalCards() {
            var cards = new List<T>();

            foreach (var manifest in GetLocalPackManifests()) {
                _logger.LogMessage($"Loading Local Cards from {manifest.Name}.");
                cards.AddRange(manifest.Cards);
            }

            return LoadLocalCardsImpl(null);
        }

        public List<T> LoadLocalCardsFromPacks(IList<string> packsToLoad) {
            return LoadLocalCardsImpl(packsToLoad);
        }
        public IEnumerable<T> AllCards() {
            return from manifest in GetLocalPackManifests()
                   from card in manifest.Cards
                   select card;
        }

        public void InvalidateManifestCache() {
            _cachedManifests = null;
        }


        public IList<LocalPackManifest<T>> GetLocalPackManifests() {
            if (_cachedManifests != null) {
                return _cachedManifests;
            }

            var manifests = new List<LocalPackManifest<T>>();

            if (!Directory.Exists(_appData.Configuration.LocalImagesDirectory)) {
                return manifests;
            }

            _logger.LogMessage("Loading local pack manifests");
            try {
                foreach (var directory in Directory.GetDirectories(_appData.Configuration.LocalImagesDirectory)) {
                    var manifestPath = directory + "\\Manifest.json";
                    if (File.Exists(manifestPath)) {
                        manifests.Add(JsonConvert.DeserializeObject<LocalPackManifest<T>>(File.ReadAllText(manifestPath)));
                    }
                }
                _cachedManifests = manifests;
            } catch (Exception e) {
                _logger.LogException(e, "Error loading local pack manifests");
            }

            return manifests;
        }

        private List<T> LoadLocalCardsImpl(IList<string> packsToLoad) {
            var cards = new List<T>();

            foreach (var manifest in GetLocalPackManifests()) {
                if (packsToLoad != null && !packsToLoad.Any(x => string.Equals(x, manifest.Name, StringComparison.InvariantCulture))) {
                    continue;
                }

                _logger.LogMessage($"Loading Local Cards from {manifest.Name}.");
                cards.AddRange(manifest.Cards);
            }

            return cards;
        }
    }
}
