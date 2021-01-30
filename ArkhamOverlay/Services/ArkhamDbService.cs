using ArkhamOverlay.Data;
using ArkhamOverlay.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace ArkhamOverlay.Services {
    public class ArkhamDbService {
        private readonly LoggingService _logger;
        private readonly AppData _appData;
        private readonly LocalCardsService _localCardsService;

        public ArkhamDbService(LoggingService loggingService, AppData appData, LocalCardsService localCardsService) {
            _logger = loggingService;
            _appData = appData;
            _localCardsService = localCardsService;
        }

        internal void LoadPlayer(Player player) {
            if (string.IsNullOrEmpty(player.DeckId)) {
                _logger.LogWarning($"{player.ID} has no deck ID.");
                return;
            }

            _logger.LogMessage($"Loading deck {player.DeckId} for player {player.ID}.");

            string url = @"https://arkhamdb.com/api/public/deck/" + player.DeckId;

            var request = (HttpWebRequest)WebRequest.Create(url);

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream)) {
                var arkhamDbDeck = JsonConvert.DeserializeObject<ArkhamDbDeck>(reader.ReadToEnd());
                player.SelectableCards.Name = arkhamDbDeck.Investigator_Name;
                player.InvestigatorCode = arkhamDbDeck.Investigator_Code;
                player.Slots = arkhamDbDeck.Slots;
                var localCard = _localCardsService.GetCardById(arkhamDbDeck.Investigator_Code);
                if (localCard != null) {
                    player.LoadImage(localCard.FilePath);
                } else {
                    player.LoadImage("https://arkhamdb.com/bundles/cards/" + arkhamDbDeck.Investigator_Code + ".png");
                }
            }

            _logger.LogMessage($"Loading investigator card for player {player.ID}.");

            var investigatorUrl = @"https://arkhamdb.com/api/public/card/" + player.InvestigatorCode;
            var investigatorRequest = (HttpWebRequest)WebRequest.Create(investigatorUrl);
            using (var response = (HttpWebResponse)investigatorRequest.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream)) {
                var arkhamDbCard = JsonConvert.DeserializeObject<ArkhamDbCard>(reader.ReadToEnd());

                if (Enum.TryParse(arkhamDbCard.Faction_Name, ignoreCase: true, out Faction faction)) {
                    player.Faction = faction;
                } else {
                    _logger.LogWarning($"Could not parse faction {arkhamDbCard.Faction_Name}.");
                }
            }

            _logger.LogMessage($"Finished loading player {player.ID}.");

            player.OnPlayerChanged();
        }

        internal void LoadPlayerCards(Player player) {
            if (player.Slots == null) {
                _logger.LogWarning($"{player.ID} has no cards in deck.");
                return;
            }

            _logger.LogMessage($"Loading cards for player {player.ID}.");

            player.SelectableCards.Loading = true;
            try {
                var cards = new List<Card>();
                foreach (var slot in player.Slots) {
                    var baseCardUrl = @"https://arkhamdb.com/api/public/card/";
                    HttpWebRequest cardRequest = (HttpWebRequest)WebRequest.Create(baseCardUrl + slot.Key);

                    using (HttpWebResponse cardRsponse = (HttpWebResponse)cardRequest.GetResponse())
                    using (Stream cardStream = cardRsponse.GetResponseStream())
                    using (StreamReader cardReader = new StreamReader(cardStream)) {
                        var arkhamDbCard = JsonConvert.DeserializeObject<ArkhamDbCard>(cardReader.ReadToEnd());
                        CheckForLocalImages(arkhamDbCard);
                        cards.Add(new Card(arkhamDbCard, slot.Value, true));
                    }
                }
                player.SelectableCards.LoadCards(cards);
            }
            catch (Exception ex) {
                _logger.LogException(ex, $"Error loading cards for player {player.ID}.");
            } finally {
                player.SelectableCards.Loading = false;
            }
            _logger.LogMessage($"Finished loading cards for player {player.ID}.");
        }

        private void CheckForLocalImages(ArkhamDbCard arkhamDbCard) {
            var localCard = _localCardsService.GetCardById(arkhamDbCard.Code);
            if (localCard != null) {
                arkhamDbCard.ImageSrc = localCard.FilePath;
                if (localCard.HasBack) {
                    arkhamDbCard.BackImageSrc = localCard.BackFilePath;
                }
            } else {
                if (!string.IsNullOrEmpty(arkhamDbCard.ImageSrc)) arkhamDbCard.ImageSrc = "https://arkhamdb.com/" + arkhamDbCard.ImageSrc;
                if (!string.IsNullOrEmpty(arkhamDbCard.BackImageSrc)) arkhamDbCard.ImageSrc = "https://arkhamdb.com/" + arkhamDbCard.BackImageSrc;
            }
        }

        internal void FindMissingEncounterSets(Configuration configuration) {
            var packsUrl = @"https://arkhamdb.com/api/public/packs/";
            HttpWebRequest cardRequest = (HttpWebRequest)WebRequest.Create(packsUrl);

            _logger.LogMessage("Looking for encounter sets.");

            var setsAdded = false;

            using (HttpWebResponse cardRsponse = (HttpWebResponse)cardRequest.GetResponse())
            using (Stream cardStream = cardRsponse.GetResponseStream())
            using (StreamReader cardReader = new StreamReader(cardStream)) {
                var packs = JsonConvert.DeserializeObject<List<ArkhamDbPack>>(cardReader.ReadToEnd());
                foreach (var pack in packs) {
                    if (!configuration.Packs.Any(x => x.Code == pack.Code)) {
                        if (AddPackToConfiguration(configuration, pack)) {
                            setsAdded = true;
                        }
                    }
                }
            }

            _logger.LogMessage($"Found new encounter sets: {setsAdded}.");

            if (setsAdded) {
                configuration.OnConfigurationChange();
            }
        }

        internal bool AddPackToConfiguration(Configuration configuration, ArkhamDbPack arkhamDbPack) {
            var cards = GetCardsInPack(arkhamDbPack.Code);
            var encounterSets = new List<EncounterSet>();
            foreach (var card in cards) {
                if (string.IsNullOrEmpty(card.Encounter_Code) || encounterSets.Any(x => x.Code == card.Encounter_Code)) {
                    continue;
                }

                encounterSets.Add(new EncounterSet { Code = card.Encounter_Code, Name = card.Encounter_Name });
            }

            if (!encounterSets.Any()) {
                return false;
            }

            var newPack = new Pack { 
                Code = arkhamDbPack.Code, 
                Name = arkhamDbPack.Name,
                CyclePosition = arkhamDbPack.Cycle_Position,
                Position = arkhamDbPack.Position,
                EncounterSets = encounterSets
            };

            configuration.Packs.Add(newPack);

            _logger.LogMessage($"Added pack {newPack.Name} to encounter sets.");

            return true;
        }

        internal List<Card> LoadEncounterCards() {
            var packsToLoad = new List<Pack>();
            // TODO: pass in list of packs to load and remove dependency on appdata
            foreach (var pack in _appData.Configuration.Packs) {
                foreach (var encounterSet in pack.EncounterSets) {
                    if (_appData.Game.IsEncounterSetSelected(encounterSet.Code)) {
                        packsToLoad.Add(pack);
                        break;
                    }
                }
            }

            var cards = new List<Card>();
            foreach (var pack in packsToLoad) {
                var arkhamDbCards = GetCardsInPack(pack.Code);

                foreach (var arkhamDbCard in arkhamDbCards) {
                    if (!_appData.Game.IsEncounterSetSelected(arkhamDbCard.Encounter_Code)) {
                        continue;
                    }

                    CheckForLocalImages(arkhamDbCard);

                    var newCard = new Card(arkhamDbCard, 1, false);
                    cards.Add(newCard);
                    if (!string.IsNullOrEmpty(arkhamDbCard.BackImageSrc)) {
                        var newCardBack = new Card(arkhamDbCard, 1, false, true);
                        newCard.FlipSideCard = newCardBack;
                        newCardBack.FlipSideCard = newCard;
                        cards.Add(newCardBack);
                    }
                }
            }

            return cards;
        }

        internal IList<ArkhamDbCard> GetCardsInPack(string packCode) {
            var cardsInPackUrl = @"https://arkhamdb.com/api/public/cards/" + packCode;
            HttpWebRequest cardRequest = (HttpWebRequest)WebRequest.Create(cardsInPackUrl);

            using (HttpWebResponse cardRsponse = (HttpWebResponse)cardRequest.GetResponse())
            using (Stream cardStream = cardRsponse.GetResponseStream())
            using (StreamReader cardReader = new StreamReader(cardStream)) {
                return JsonConvert.DeserializeObject<IList<ArkhamDbCard>>(cardReader.ReadToEnd());
            }
        }
    }
}
