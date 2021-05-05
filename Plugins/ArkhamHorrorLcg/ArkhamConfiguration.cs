using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ArkhamHorrorLcg {
    interface IArkhamConfiguration {
        string LocalImagesDirectory { get; set; }

        IList<Pack> Packs { get; }

        void Save();
    }

    public class ArkhamConfiguration : IArkhamConfiguration {
        private readonly ILoggingService _logger;
        private readonly string _configFileName = ArkhamHorrorLcg.PlugInName + ".json";

        public ArkhamConfiguration(ILoggingService logger) {
            _logger = logger;

            Packs = new List<Pack>();

            _logger.LogMessage("Loading Arkham configuration");

            if (File.Exists(_configFileName)) {
                _logger.LogMessage("Found Arkham configuration file.");
                try {
                    var file = JsonConvert.DeserializeObject<ConfigurationFile>(File.ReadAllText(_configFileName));
                    Packs = file.Packs;
                    LocalImagesDirectory = file.LocalImagesDirectory;
                } catch (Exception ex) {
                    // if there's an error, we don't care- just use an empty configuration
                    _logger.LogException(ex, "Error reading Arkham configuration file.");
                }
            } else {
                _logger.LogMessage("No Arkham configuration file found");
            }
        }

        public IList<Pack> Packs { get; }

        public string LocalImagesDirectory { get; set; }

        public void Save() {
            _logger.LogMessage("Saving Arkham configuration to file.");
            var file = new ConfigurationFile {
                Packs = Packs,
                LocalImagesDirectory = LocalImagesDirectory
            };
            try {
                File.WriteAllText(_configFileName, JsonConvert.SerializeObject(file));
            } catch (Exception ex) {
                _logger.LogException(ex, "Error saving Arkham configuration to file.");
            }
        }
    }

    public class ConfigurationFile {
        public IList<Pack> Packs { get; set; }
        public string LocalImagesDirectory { get; set; }
    }
}
