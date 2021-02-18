using ArkhamOverlay.Common.Events;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Data;
using ArkhamOverlay.Pages.Main;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ArkhamOverlay.Services {
    public interface IGame {
        string Name { get; set; }
        string Scenario { get; set; }
        string SnapshotDirectory { get; set; }
        IList<EncounterSet> EncounterSets { get; }
        IList<string> LocalPacks { get; }
    }

    public class GameFile : IGame {
        public GameFile() {
            EncounterSets = new List<EncounterSet>();
            LocalPacks = new List<string>();
            DeckIds = new List<string>();
        }

        public string Name { get; set; }

        public string Scenario { get; set; }
        public string SnapshotDirectory { get; set; }

        public IList<string> DeckIds { get; set; }

        public IList<EncounterSet> EncounterSets { get; set; }

        public IList<string> LocalPacks { get; set; }
    }

    public class GameFileService {
        private readonly AppData _appData;
        private readonly CardLoadService _cardLoadService;
        private readonly LoadingStatusService _loadingStatusService;
        private readonly LoggingService _logger;
        private readonly IEventBus _eventBus;

        public GameFileService(AppData appData, CardLoadService cardLoadService, LoadingStatusService loadingStatusService, LoggingService loggingService, IEventBus eventBus) {
            _appData = appData;
            _cardLoadService = cardLoadService;
            _loadingStatusService = loadingStatusService;
            _logger = loggingService;
            _eventBus = eventBus;
        }

        internal void Load(string fileName) {
            _logger.LogMessage($"Loading game from file {fileName}.");
            if (File.Exists(fileName)) {
                try {
                    _eventBus.PublishClearAllCardsRequest();
                    var gameFile = JsonConvert.DeserializeObject<GameFile>(File.ReadAllText(fileName));

                    var game = _appData.Game;
                    game.ClearAllCardsLists();

                    gameFile.CopyTo(game);
                    game.OnEncounterSetsChanged();

                    for (var index = 0; index < gameFile.DeckIds.Count && index < game.Players.Count; index++) {
                        game.Players[index].DeckId = gameFile.DeckIds[index];
                        if (!string.IsNullOrEmpty(game.Players[index].DeckId)) {
                            try {
                                _loadingStatusService.ReportPlayerStatus(game.Players[index].ID, Status.LoadingCards);
                                _cardLoadService.LoadPlayer(game.Players[index]);
                            }
                            catch (Exception ex) {
                                _logger.LogException(ex, $"Error loading player {game.Players[index].ID}.");
                                _loadingStatusService.ReportPlayerStatus(game.Players[index].ID, Status.Error);
                            }
                        }
                    }

                    game.OnPlayersChanged();
                    _appData.OnGameChanged();
                    _logger.LogMessage($"Finished reading game file: {fileName}.");
                } catch (Exception ex) {
                    // if there's an error, we don't care- just use the existing game
                    _logger.LogException(ex, $"Error reading game file: {fileName}.");
                }
            }
        }

        internal void Save(string fileName) {
            _logger.LogMessage($"Saving game file: {fileName}.");
            var gameFile = new GameFile();
            _appData.Game.CopyTo(gameFile);
            foreach (var player in _appData.Game.Players) {
                gameFile.DeckIds.Add(player.DeckId);
            }

            try {
                File.WriteAllText(fileName, JsonConvert.SerializeObject(gameFile));
                File.WriteAllText("LastSaved.json", JsonConvert.SerializeObject(gameFile));
                _logger.LogMessage($"Finished writing to game file: {fileName}.");
            } catch (Exception ex) {
                _logger.LogException(ex, $"Error saving game file: {fileName}.");
            }
        }
    }

    public static class GameFileExtensions {
        public static void CopyTo(this IGame fromGame, IGame toGame) {
            toGame.Name = fromGame.Name;
            toGame.Scenario = fromGame.Scenario;
            toGame.SnapshotDirectory = fromGame.SnapshotDirectory;

            toGame.EncounterSets.Clear();
            foreach (var encounterSet in fromGame.EncounterSets) {
                toGame.EncounterSets.Add(new EncounterSet(encounterSet));
            }

            toGame.LocalPacks.Clear();
            foreach (var localPack in fromGame.LocalPacks) {
                toGame.LocalPacks.Add(localPack);
            }
        }
    }
}
