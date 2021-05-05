using EideticMemoryOverlay.PluginApi;
using Emo.Common.Enums;
using Emo.Common.Events;
using Emo.Common.Services;
using Emo.Data;
using Emo.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Emo.Services {
    public interface IGame {
        string PlugInName { get; set; }
        string Name { get; set; }
        string Scenario { get; set; }
        string SnapshotDirectory { get; set; }
        IList<EncounterSet> EncounterSets { get; }
        IList<string> LocalPacks { get; }
    }

    /// <summary>
    /// Information about a button in a zone for saving and recreating state
    /// </summary>
    public class ZoneButton {
        /// <summary>
        /// Card Group the button belongs to
        /// </summary>
        public CardGroupId CardGroupId { get; set; }

        /// <summary>
        /// Zone in the card group that the button belongs to
        /// </summary>
        public int ZoneIndex { get; set; }

        /// <summary>
        /// Name of the button
        /// </summary>
        public string Name { get; set; } 
    }

    public class GameFile : IGame {
        public GameFile() {
            EncounterSets = new List<EncounterSet>();
            LocalPacks = new List<string>();
            DeckIds = new List<string>();
            ZoneButtons = new List<ZoneButton>();
        }

        public string PlugInName { get; set; }
        public string Name { get; set; }
        public string Scenario { get; set; }
        public string SnapshotDirectory { get; set; }
        public IList<string> DeckIds { get; set; }
        public IList<EncounterSet> EncounterSets { get; set; }
        public IList<string> LocalPacks { get; set; }
        public IList<ZoneButton> ZoneButtons { get; set; }
    }

    public interface IGameFileService {
        void Load(string fileName = "");
        void Save(string fileName);
    }

    internal class GameFileService : IGameFileService {
        private readonly AppData _appData;
        private readonly LoadingStatusService _loadingStatusService;
        private readonly LoggingService _logger;
        private readonly IEventBus _eventBus;
        private readonly IPlugInService _plugInService;
        private readonly PlugInWrapper _plugInWrapper;
        private IList<ZoneButton> _zoneButtons;

        public GameFileService(AppData appData, LoadingStatusService loadingStatusService, LoggingService loggingService, IEventBus eventBus, IPlugInService plugInService, IPlugIn plugIn) {
            _appData = appData;
            _loadingStatusService = loadingStatusService;
            _logger = loggingService;
            _eventBus = eventBus;
            _plugInService = plugInService;
            _plugInWrapper = (PlugInWrapper)plugIn;
            eventBus.SubscribeToCardGroupButtonsChanged(CardGroupButtonsChangedHandler);
        }

        public void Load(string fileName = "") {
            if (string.IsNullOrEmpty(fileName)) {
                fileName = _appData.Configuration.LastSavedFileName;
            }

            _logger.LogMessage($"Loading game from file {fileName}.");
            if (!File.Exists(fileName)) {
                LoadPlugIn(null);
                return; 
            } 

            try {
                _eventBus.PublishClearAllCardsRequest();
                var gameFile = JsonConvert.DeserializeObject<GameFile>(File.ReadAllText(fileName));
                LoadPlugIn(gameFile.PlugInName);

                _zoneButtons = gameFile.ZoneButtons;

                var game = _appData.Game as Game;
                if (game == default) {
                    return;
                }
                game.ClearAllCardsLists();

                gameFile.CopyTo(game);
                game.FileName = fileName;

                for (var index = 0; index < gameFile.DeckIds.Count && index < game.Players.Count; index++) {
                    game.Players[index].DeckId = gameFile.DeckIds[index];
                    if (!string.IsNullOrEmpty(game.Players[index].DeckId)) {
                        try {
                            _loadingStatusService.ReportPlayerStatus(game.Players[index].ID, Status.LoadingCards);
                            _plugInWrapper.LoadPlayer(game.Players[index]);
                        } catch (Exception ex) {
                            _logger.LogException(ex, $"Error loading player {game.Players[index].ID}.");
                            _loadingStatusService.ReportPlayerStatus(game.Players[index].ID, Status.Error);
                        }
                    }
                }

                _plugInWrapper.LoadEncounterCards();
                _plugInWrapper.LoadAllPlayerCards();
                _appData.OnGameChanged();
                _logger.LogMessage($"Finished reading game file: {fileName}.");

                _appData.Configuration.LastSavedFileName = fileName;

                return;
            } catch (Exception ex) {
                // if there's an error, we don't care- just use the existing game
                _logger.LogException(ex, $"Error reading game file: {fileName}.");
                LoadPlugIn(null);
                return;
            }
        }

        private void LoadPlugIn(string plugInName) {
            if (string.IsNullOrEmpty(plugInName)) {
                plugInName = "EmoPlugIn.ArkhamHorrorLcg.dll";
            }

            var plugIn = _plugInService.GetPlugInByName(plugInName);
            if (plugIn == default) {
                _logger.LogMessage($"PlugIn {plugInName} not found.");
                plugIn = _plugInService.GetPlugInByName("EmoPlugIn.ArkhamHorrorLcg.dll");
            }

            _plugInWrapper.SetPlugIn(plugIn);

            if (plugIn == default) {
                return;
            }

            _appData.Game.InitializeFromPlugin();
        }

        public void Save(string fileName) {
            _logger.LogMessage($"Saving game file: {fileName}.");
            var gameFile = new GameFile();
            var game = _appData.Game as IGame;
            if (game == default) {
                return;
            }

            game.CopyTo(gameFile);

            foreach (var player in _appData.Game.Players) {
                gameFile.DeckIds.Add(player.DeckId);
            }

            AddDeckIdsToGameFile(gameFile);
            AddZoneButtonsToGameFile(gameFile);

            try {
                File.WriteAllText(fileName, JsonConvert.SerializeObject(gameFile));
                _appData.Configuration.LastSavedFileName = fileName;
                _logger.LogMessage($"Finished writing to game file: {fileName}.");
            } catch (Exception ex) {
                _logger.LogException(ex, $"Error saving game file: {fileName}.");
            }
        }

        /// <summary>
        /// Add all deck ids to the game file so they can be recreated
        /// </summary>
        /// <param name="gameFile">Add the deck ids to this file</param>
        private void AddDeckIdsToGameFile(GameFile gameFile) {
            foreach (var player in _appData.Game.Players) {
                gameFile.DeckIds.Add(player.DeckId);
            }
        }

        /// <summary>
        /// Save all buttons assigned to zones to the game file so the state can be recreated
        /// </summary>
        /// <param name="gameFile">Add the buttons to this file</param>
        private void AddZoneButtonsToGameFile(GameFile gameFile) {
            foreach (var cardGroup in _appData.Game.AllCardGroups) {
                foreach (var cardZone in cardGroup.CardZones) {
                    foreach (var button in cardZone.CardButtons) {
                        gameFile.ZoneButtons.Add(new ZoneButton {
                            CardGroupId = cardGroup.Id,
                            ZoneIndex = cardZone.ZoneIndex,
                            Name = button.CardInfo.Name
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Called when cards are loaded for a card group
        /// </summary>
        /// <param name="eventData">List of the cards that have changed</param>
        private void CardGroupButtonsChangedHandler(CardGroupButtonsChanged eventData) {
            if (_zoneButtons == default) {
                return;
            }

            var zoneButtonsFound = new List<ZoneButton>();

            //look through all the zone buttons and if this group of changed (presumably added) buttons contains a match we can use to create the button
            foreach (var zoneButton in _zoneButtons) {
                var matchingButton = eventData.Buttons.FirstOrDefault(x => x.Name == zoneButton.Name);
                if (matchingButton == default) {
                    continue;
                }
                
                //we'll need to remove this once we are done, so we don't keep trying to add it
                zoneButtonsFound.Add(zoneButton);

                var cardGroup = _appData.Game.GetCardGroup(matchingButton.CardGroupId);
                var button = cardGroup.GetButton(matchingButton) as CardImageButton;
                _appData.Game.AddCardToZone(zoneButton.CardGroupId, zoneButton.ZoneIndex, button);
            }

            //once we've loaded this zone button we can stop loading it
            foreach (var zoneButtonFound in zoneButtonsFound) {
                _zoneButtons.Remove(zoneButtonFound);
            }
        }
    }

    public static class GameFileExtensions {
        public static void CopyTo(this IGame fromGame, IGame toGame) {
            toGame.PlugInName = fromGame.PlugInName;
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
