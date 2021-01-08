using ArkhamOverlay.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ArkhamOverlay.Services {
    public interface IConfiguration {
        int OverlayHeight { get; set; }
        int OverlayWidth { get; set; }
        int CardHeight { get; set; }
        IList<Pack> Packs { get; }
    }

    public class ConfigurationFile : IConfiguration {
        public ConfigurationFile() {
            Packs = new List<Pack>();
        }
                
        public int OverlayHeight { get; set; }
        public int OverlayWidth { get; set; }
        public int CardHeight { get; set; }
        public bool UseActAgendaBar { get; set; }
        public IList<Pack> Packs { get; set; }
    }

    public class ConfigurationService {
        private readonly Configuration _configuration;
        public ConfigurationService(AppData appData) {
            _configuration = appData.Configuration;
        }

        public void Load() {
            var configuration = new ConfigurationFile {
                OverlayWidth = 1228,
                OverlayHeight = 720,
                CardHeight = 300
            };

            if (File.Exists("Config.json")) {
                try {
                    configuration = JsonConvert.DeserializeObject<ConfigurationFile>(File.ReadAllText("Config.json"));
                } catch {
                    // if there's an error, we don't care- just use the default configuration
                }
            }
            configuration.CopyTo(_configuration);

            _configuration.ConfigurationChanged += Save;
        }

        private void Save() {
            var configurationFile = new ConfigurationFile();
            _configuration.CopyTo(configurationFile);

            File.WriteAllText("Config.json", JsonConvert.SerializeObject(configurationFile));
        }
    }

    public static class ConfigurationExtensions {
        public static void CopyTo(this IConfiguration fromConfiguration, IConfiguration toConfiguration) {
            toConfiguration.OverlayHeight = fromConfiguration.OverlayHeight;
            toConfiguration.OverlayWidth = fromConfiguration.OverlayWidth;
            toConfiguration.CardHeight = fromConfiguration.CardHeight;
            toConfiguration.Packs.Clear();
            foreach (var pack in fromConfiguration.Packs) {
                toConfiguration.Packs.Add(new Pack(pack));
            }
        }
    }
}
