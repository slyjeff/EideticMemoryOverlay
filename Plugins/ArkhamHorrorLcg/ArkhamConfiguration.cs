using EideticMemoryOverlay.PluginApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ArkhamHorrorLcg {
    interface IArkhamConfiguration {
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
                } catch (Exception ex) {
                    // if there's an error, we don't care- just use an empty configuration
                    _logger.LogException(ex, "Error reading Arkham configuration file.");
                }
            } else {
                _logger.LogMessage("No Arkham configuration file found");
            }
        }

        public IList<Pack> Packs { get; }

        public void Save() {
            _logger.LogMessage("Saving Arkham configuration to file.");
            var file = new ConfigurationFile {
                Packs = Packs
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
    }

    public class Pack {
        public Pack() {
            EncounterSets = new List<EncounterSet>();
        }

        public Pack(Pack pack) {
            Code = pack.Code;
            Name = pack.Name;
            CyclePosition = pack.CyclePosition;
            Position = pack.Position;

            EncounterSets = new List<EncounterSet>();
            foreach (var encounterSet in pack.EncounterSets) {
                EncounterSets.Add(new EncounterSet(encounterSet));
            }
        }

        public string Code { get; set; }

        public string Name { get; set; }

        public int CyclePosition { get; set; }

        public int Position { get; set; }

        public IList<EncounterSet> EncounterSets { get; set; }
    }

    public class EncounterSet {
        public EncounterSet() {
        }

        public EncounterSet(EncounterSet encounterSet) {
            Name = encounterSet.Name;
            Code = encounterSet.Code;
        }

        public string Name { get; set; }
        public string Code { get; set; }
    }
}
