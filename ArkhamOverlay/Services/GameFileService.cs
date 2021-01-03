using ArkhamOverlay.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ArkhamOverlay.Services {
    public interface IGame {
        string Name { get; set; }

        string Scenario { get; set; }

        IList<EncounterSet> EncounterSets { get; }
    }

    public class GameFile : IGame {
        public GameFile() {
            EncounterSets = new List<EncounterSet>();
            DeckIds = new List<string>();
        }

        public string Name { get; set; }

        public string Scenario { get; set; }

        public IList<string> DeckIds { get; set; }

        public IList<EncounterSet> EncounterSets { get; set; }
    }

    public class GameFileService {
        private readonly AppData _appData;
        private readonly ArkhamDbService _arkhamDbService = new ArkhamDbService();

        public GameFileService(AppData appData) {
            _appData = appData;
        }

        internal void Load(string fileName) {
            if (File.Exists(fileName)) {
                try {
                    var gameFile = JsonConvert.DeserializeObject<GameFile>(File.ReadAllText(fileName));

                    var game = _appData.Game;
                    game.ClearAllCardsLists();

                    gameFile.CopyTo(game);
                    game.OnEncounterSetsChanged();

                    for (var index = 0; index < gameFile.DeckIds.Count && index < game.Players.Count; index++) {
                        game.Players[index].DeckId = gameFile.DeckIds[index];
                        _arkhamDbService.LoadPlayer(game.Players[index]);
                    }

                    game.OnPlayersChanged();

                    _appData.OnGameChanged();
                } catch {
                    // if there's an error, we don't care- just use the existing game
                }
            }
        }

        internal void Save(string fileName) {
            var gameFile = new GameFile();
            _appData.Game.CopyTo(gameFile);
            foreach (var player in _appData.Game.Players) {
                gameFile.DeckIds.Add(player.DeckId);
            }

            File.WriteAllText(fileName, JsonConvert.SerializeObject(gameFile));
            File.WriteAllText("LastSaved.json", JsonConvert.SerializeObject(gameFile));
        }
    }

    public static class GameFileExtensions {
        public static void CopyTo(this IGame fromGame, IGame toGame) {
            toGame.Name = fromGame.Name;
            toGame.Scenario = fromGame.Scenario;
            toGame.EncounterSets.Clear();
            foreach (var encounterSet in fromGame.EncounterSets) {
                toGame.EncounterSets.Add(new EncounterSet(encounterSet));
            }
        }
    }
}
