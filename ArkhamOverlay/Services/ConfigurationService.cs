using ArkhamOverlay.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace ArkhamOverlay.Services {
    public interface IConfiguration {
        bool TrackHealthAndSanity { get; set; }
        bool TrackResources { get; set; }
        bool TrackClues { get; set; }
        bool SeperateStatSnapshots { get; set; }
        Color OverlayColor { get; set; }
        int OverlayHeight { get; set; }
        int OverlayWidth { get; set; }
        int CardHeight { get; set; }
        int ActAgendaCardHeight { get; set; }
        int HandCardHeight { get; set; }
        Point ScenarioCardsPosition { get; set; }
        Point LocationsPosition { get; set; }
        Point EncounterCardsPosition { get; set; }
        Point Player1Position { get; set; }
        Point Player2Position { get; set; }
        Point Player3Position { get; set; }
        Point Player4Position { get; set; }
        Point OverlayPosition { get; set; }
        bool UseAutoSnapshot { get; set; }
        string AutoSnapshotFilePath { get; set; }
        string LocalImagesDirectory { get; set; }
        IList<Pack> Packs { get; }
    }

    public class ConfigurationFile : IConfiguration {
        public ConfigurationFile() {
            Packs = new List<Pack>();
        }

        public bool TrackHealthAndSanity { get; set; }
        public bool TrackResources { get; set; }
        public bool TrackClues { get; set; }
        public bool SeperateStatSnapshots { get; set; }
        public Color OverlayColor { get; set; }
        public int OverlayHeight { get; set; }
        public int OverlayWidth { get; set; }
        public int CardHeight { get; set; }
        public int ActAgendaCardHeight { get; set; }
        public int HandCardHeight { get; set; }
        public bool UseActAgendaBar { get; set; }
        public IList<Pack> Packs { get; set; }
        public Point ScenarioCardsPosition { get; set; }
        public Point LocationsPosition { get; set; }
        public Point EncounterCardsPosition { get; set; }
        public Point Player1Position { get; set; }
        public Point Player2Position { get; set; }
        public Point Player3Position { get; set; }
        public Point Player4Position { get; set; }
        public Point OverlayPosition { get; set; }
        public bool UseAutoSnapshot { get; set; }
        public string AutoSnapshotFilePath { get; set; }
        public string LocalImagesDirectory { get; set; }
    }

    public class ConfigurationService {
        public static Color DefaultBackgroundColor = (Color)ColorConverter.ConvertFromString("#00B164");

        private readonly Configuration _configuration;
        private readonly LoggingService _logger;

        public ConfigurationService(AppData appData, LoggingService loggingService) {
            _configuration = appData.Configuration;
            _logger = loggingService;
        }

        public void Load() {
            _logger.LogMessage("Loading configuration");
            var configuration = new ConfigurationFile {
                OverlayColor = DefaultBackgroundColor,
                OverlayWidth = 1228,
                OverlayHeight = 720,
                CardHeight = 300,
                ActAgendaCardHeight = 200,
                HandCardHeight = 200
            };

            if (File.Exists("Config.json")) {
                _logger.LogMessage("Found configuration file.");
                try {
                    configuration = JsonConvert.DeserializeObject<ConfigurationFile>(File.ReadAllText("Config.json"));
                } catch (Exception ex) {
                    // if there's an error, we don't care- just use the default configuration
                    _logger.LogException(ex, "Error reading configuration file.");
                }
            } else {
                _logger.LogMessage("No configuration file found");
            }
            configuration.CopyTo(_configuration);

            _configuration.ConfigurationChanged += Save;
        }

        private void Save() {
            _logger.LogMessage("Saving configuration to file.");
            var configurationFile = new ConfigurationFile();
            _configuration.CopyTo(configurationFile);
            try {
                File.WriteAllText("Config.json", JsonConvert.SerializeObject(configurationFile));
            } catch (Exception ex) {
                _logger.LogException(ex, "Error saving configuration to file.");
            }
        }
    }

    public static class ConfigurationExtensions {
        public static void CopyTo(this IConfiguration fromConfiguration, IConfiguration toConfiguration) {
            toConfiguration.TrackHealthAndSanity = fromConfiguration.TrackHealthAndSanity;
            toConfiguration.TrackResources = fromConfiguration.TrackResources;
            toConfiguration.TrackClues = fromConfiguration.TrackClues;
            toConfiguration.SeperateStatSnapshots = fromConfiguration.SeperateStatSnapshots;
            toConfiguration.OverlayColor = fromConfiguration.OverlayColor;
            toConfiguration.OverlayHeight = fromConfiguration.OverlayHeight;
            toConfiguration.OverlayWidth = fromConfiguration.OverlayWidth;
            toConfiguration.CardHeight = fromConfiguration.CardHeight;
            toConfiguration.ActAgendaCardHeight = fromConfiguration.ActAgendaCardHeight;
            toConfiguration.HandCardHeight = fromConfiguration.HandCardHeight;
            toConfiguration.ScenarioCardsPosition = fromConfiguration.ScenarioCardsPosition;
            toConfiguration.LocationsPosition = fromConfiguration.LocationsPosition;
            toConfiguration.EncounterCardsPosition = fromConfiguration.EncounterCardsPosition;
            toConfiguration.Player1Position = fromConfiguration.Player1Position;
            toConfiguration.Player2Position = fromConfiguration.Player2Position;
            toConfiguration.Player3Position = fromConfiguration.Player3Position;
            toConfiguration.Player4Position = fromConfiguration.Player4Position;
            toConfiguration.OverlayPosition = fromConfiguration.OverlayPosition;
            toConfiguration.UseAutoSnapshot = fromConfiguration.UseAutoSnapshot;
            toConfiguration.AutoSnapshotFilePath= fromConfiguration.AutoSnapshotFilePath;
            toConfiguration.LocalImagesDirectory = fromConfiguration.LocalImagesDirectory;
            toConfiguration.Packs.Clear();
            foreach (var pack in fromConfiguration.Packs) {
                toConfiguration.Packs.Add(new Pack(pack));
            }
        }
    }
}
