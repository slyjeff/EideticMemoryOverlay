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

            var playerCard = GetCard(player.InvestigatorCode);

            if (Enum.TryParse(playerCard.Faction_Name, ignoreCase: true, out Faction faction)) {
                player.Faction = faction;
            } else {
                _logger.LogWarning($"Could not parse faction {playerCard.Faction_Name}.");
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
                    var arkhamDbCard = GetCard(slot.Key);
                    CheckForLocalImages(arkhamDbCard);
                    cards.Add(new Card(arkhamDbCard, slot.Value, true));
                    var bondedCards = LocalCardCache.Instance.GetBondedCards(arkhamDbCard);
                    cards.AddRange(bondedCards.Select(c => new Card(CheckForLocalImages(c.Key), c.Value, isPlayerCard:true, isBonded:true)));
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

        private ArkhamDbCard CheckForLocalImages(ArkhamDbCard arkhamDbCard) {
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

            return arkhamDbCard;
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

        private ArkhamDbCard GetCard(string cardCode) {
            var baseCardUrl = @"https://arkhamdb.com/api/public/card/";
            HttpWebRequest cardRequest = (HttpWebRequest)WebRequest.Create(baseCardUrl + cardCode);
            try {
                using (HttpWebResponse cardRsponse = (HttpWebResponse)cardRequest.GetResponse())
                using (Stream cardStream = cardRsponse.GetResponseStream())
                using (StreamReader cardReader = new StreamReader(cardStream)) {
                    var arkhamDbCard = JsonConvert.DeserializeObject<ArkhamDbCard>(cardReader.ReadToEnd());
                    return arkhamDbCard;
                }
            } catch (Exception ex) {
                _logger.LogException(ex, $"Error fetching card with code: {cardCode}");
                return null;
            }
        }

        private class LocalCardCache {
            public static LocalCardCache Instance = new LocalCardCache();

            private object syncObject = new object();
            private bool initialized = false;
            private List<ArkhamDbFullCard> allCards = new List<ArkhamDbFullCard>();

            private LocalCardCache() {
            }

            public void Initialize() {
                lock (syncObject) {
                    if (!initialized) {
                        try {
                            var cardsURL = @"https://arkhamdb.com/api/public/cards/";
                            HttpWebRequest cardsRequest = (HttpWebRequest)WebRequest.Create(cardsURL);

                            using (HttpWebResponse cardRsponse = (HttpWebResponse)cardsRequest.GetResponse())
                            using (Stream cardStream = cardRsponse.GetResponseStream())
                            using (StreamReader cardReader = new StreamReader(cardStream)) {
                                allCards = JsonConvert.DeserializeObject<List<ArkhamDbFullCard>>(cardReader.ReadToEnd());
                                initialized = true;
                            }
                        } catch {
                            // Best effort attempt, do nothing if it fails.
                        }
                    }
                }
            }

            public ArkhamDbFullCard GetCard(string code) {
                Initialize();

                return allCards.FirstOrDefault(c => c.Code.Equals(code));
            }

            public Dictionary<ArkhamDbCard, int> GetBondedCards(ArkhamDbCard card) {
                Initialize();

                if (!(card is ArkhamDbFullCard mainCard)) {
                    mainCard = allCards.FirstOrDefault(c => c.Code.Equals(card.Code));
                }

                var bondedCards = new Dictionary<ArkhamDbCard, int>();
                if (mainCard != null && mainCard.Bonded_Cards != null) {
                    foreach (var bondedCard in mainCard.Bonded_Cards) {
                        bondedCards.Add(allCards.FirstOrDefault(c => c.Code.Equals(bondedCard.Code)), bondedCard.Count);
                    }
                }
                return bondedCards;
            }
        }

        public class ArkhamDbFullCard : ArkhamDbCard {
            public List<BondedCard> Bonded_Cards;
        }

        public class BondedCard {
            public int Count;
            public string Code;
        }
    }
}
