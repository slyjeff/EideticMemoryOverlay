using Emo.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Emo.Services {
    public class CardLoadService {
        private readonly AppData _appData;
        private readonly LoadingStatusService _loadingStatusService;
        private readonly ArkhamDbService _arkhamDbService;
        private readonly LocalCardsService _localCardsService;
        private readonly CardImageService _cardImageService;
        private readonly LoggingService _logger;

        public CardLoadService(ArkhamDbService arkhamDbService, LocalCardsService localCardsService, CardImageService cardImageService, AppData appData, LoadingStatusService loadingStatusService, LoggingService loggingService) {
            _arkhamDbService = arkhamDbService;
            _localCardsService = localCardsService;
            _cardImageService = cardImageService;
            _appData = appData;
            _loadingStatusService = loadingStatusService;
            _logger = loggingService;
        }

        public void FindMissingEncounterSets(Configuration configuration) {
            _logger.LogMessage("Looking for encounter sets.");

            var setsAdded = false;
            var packs = _arkhamDbService.GetAllPacks();

            foreach (var pack in packs) {
                if (!configuration.Packs.Any(x => x.Code == pack.Code)) {
                    if (AddPackToConfiguration(configuration, pack)) {
                        setsAdded = true;
                    }
                }
            }

            _logger.LogMessage($"Found new encounter sets: {setsAdded}.");

            if (setsAdded) {
                configuration.OnConfigurationChange();
            }
        }

        private bool AddPackToConfiguration(Configuration configuration, ArkhamDbPack arkhamDbPack) {
            var cards = _arkhamDbService.GetCardsInPack(arkhamDbPack.Code);
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

        public void RegisterEvents() {
            _appData.Game.PlayersChanged += LoadAllPlayerCards;
            _appData.Game.EncounterSetsChanged += LoadAllEncounterCards;
        }

        public void LoadPlayer(Player player) {
            if (string.IsNullOrEmpty(player.DeckId)) {
                _logger.LogWarning($"{player.ID} has no deck ID.");
                return;
            }

            _logger.LogMessage($"Loading deck for player {player.ID}.");

            var arkhamDbDeck = _arkhamDbService.GetPlayerDeck(player.DeckId);
            player.CardGroup.Name = arkhamDbDeck.Investigator_Name;
            player.InvestigatorCode = arkhamDbDeck.Investigator_Code;
            player.Slots = arkhamDbDeck.Slots;

            _logger.LogMessage($"Loading investigator card for player {player.ID}.");

            var playerCard = _arkhamDbService.GetCard(player.InvestigatorCode);

            player.Health.Max = playerCard.Health;
            player.Sanity.Max = playerCard.Sanity;

            FixArkhamDbCardImageSource(playerCard);
            var localCard = _localCardsService.GetCardById(arkhamDbDeck.Investigator_Code);
            if (localCard != null) {
                player.ImageSource = localCard.FilePath;
            } else {
                player.ImageSource = playerCard.ImageSrc;
            }

            _cardImageService.LoadImage(player);

            if (Enum.TryParse(playerCard.Faction_Name, ignoreCase: true, out Faction faction)) {
                player.Faction = faction;
            } else {
                _logger.LogWarning($"Could not parse faction {playerCard.Faction_Name}.");
            }

            _logger.LogMessage($"Finished loading player {player.ID}.");

            player.OnPlayerChanged();
        }

        private void LoadAllPlayerCards() {
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                foreach (var player in _appData.Game.Players) {
                    if (!string.IsNullOrEmpty(player.DeckId)) {
                        _loadingStatusService.ReportPlayerStatus(player.ID, Status.LoadingCards);
                        try {
                            LoadPlayerCards(player);

                            _loadingStatusService.ReportPlayerStatus(player.ID, Status.Finished);
                        } catch (Exception ex) {
                            _logger.LogException(ex, $"Error loading player cards for player {player.ID}");
                            _loadingStatusService.ReportPlayerStatus(player.ID, Status.Error);
                        }
                    }
                }
            };
            worker.RunWorkerAsync();
        }

        private void LoadAllEncounterCards() {
            var worker = new BackgroundWorker();
            worker.DoWork += (x, y) => {
                _loadingStatusService.ReportEncounterCardsStatus(Status.LoadingCards);
                _logger.LogMessage("Loading encounter cards.");
                try {
                    _appData.Game.ScenarioCards.Loading = true;
                    _appData.Game.LocationCards.Loading = true;
                    _appData.Game.EncounterDeckCards.Loading = true;

                    var cards = GetEncounterCards();

                    var scenarioCards = new List<CardInfo>();
                    var agendas = new List<CardInfo>();
                    var acts = new List<CardInfo>();
                    var locations = new List<CardInfo>();
                    var treacheries = new List<CardInfo>();
                    var enemies = new List<CardInfo>();

                    foreach (var card in cards) {
                        switch (card.Type) {
                            case CardType.Scenario:
                                scenarioCards.Add(card);
                                break;
                            case CardType.Agenda:
                                agendas.Add(card);
                                break;
                            case CardType.Act:
                                acts.Add(card);
                                break;
                            case CardType.Location:
                                locations.Add(card);
                                break;
                            case CardType.Treachery:
                            case CardType.Enemy:
                                treacheries.Add(card);
                                break;
                            default:
                                break;
                        }
                    }

                    scenarioCards.AddRange(agendas);
                    scenarioCards.AddRange(acts);

                    _loadingStatusService.ReportEncounterCardsStatus(Status.Finished);

                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                        _appData.Game.ScenarioCards.LoadCards(scenarioCards);
                        _appData.Game.LocationCards.LoadCards(locations);
                        _appData.Game.EncounterDeckCards.LoadCards(treacheries);
                    }));
                } catch (Exception ex) {
                    _logger.LogException(ex, "Error loading encounter cards.");
                    _loadingStatusService.ReportEncounterCardsStatus(Status.Error);
                } finally {
                    _appData.Game.ScenarioCards.Loading = false;
                    _appData.Game.LocationCards.Loading = false;
                    _appData.Game.EncounterDeckCards.Loading = false;
                }
                _logger.LogMessage($"Finished loading encounter cards.");

            };
            worker.RunWorkerAsync();
        }

        public void LoadPlayerCards(Player player) {
            if (player.Slots == null) {
                _logger.LogWarning($"{player.ID} has no cards in deck.");
                return;
            }

            _logger.LogMessage($"Loading cards for player {player.ID}.");

            // TODO: Consider alternatives to loading all cards
            var localCards = _localCardsService.LoadLocalCards();

            player.CardGroup.Loading = true;
            try {
                var cards = new List<CardInfo>();
                foreach (var slot in player.Slots) {
                    ArkhamDbCard arkhamDbCard = _arkhamDbService.GetCard(slot.Key);
                    if (arkhamDbCard != null) {

                        // Override card image with local card if possible
                        FindCardImageSource(arkhamDbCard, localCards);

                        var card = new CardInfo(arkhamDbCard, slot.Value, true);

                        _cardImageService.LoadImage(card);

                        cards.Add(card);

                        // Look for bonded cards if present
                        cards.AddRange(GetBondedCards(arkhamDbCard, localCards));
                        
                    } else {
                        _logger.LogError($"Could not find player {player.ID} card: {slot.Key}");
                    }
                }
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    player.CardGroup.LoadCards(cards);
                }));
            } catch (Exception ex) {
                _logger.LogException(ex, $"Error loading cards for player {player.ID}.");
            } finally {
                player.CardGroup.Loading = false;
            }
            _logger.LogMessage($"Finished loading cards for player {player.ID}.");
        }

        private IEnumerable<CardInfo> GetBondedCards(ArkhamDbCard arkhamDbCard, List<LocalManifestCard> localCards) {
            if (arkhamDbCard is ArkhamDbFullCard fullCard && fullCard.Bonded_Cards?.Any() == true) {
                foreach (var bondedCardInfo in fullCard.Bonded_Cards) {
                    ArkhamDbCard bondedArkhamDbCard = _arkhamDbService.GetCard(bondedCardInfo.Code);
                    if (bondedArkhamDbCard != null) {

                        // Override card image with local card if possible
                        FindCardImageSource(bondedArkhamDbCard, localCards);

                        var bondedCard = new CardInfo(bondedArkhamDbCard, bondedCardInfo.Count, isPlayerCard: true, isBonded: true);
                        _cardImageService.LoadImage(bondedCard);
                        yield return bondedCard;
                    } else {
                        _logger.LogError($"Could not find bonded card: {bondedCardInfo.Code}, bonded to: {arkhamDbCard.Code}");
                    }
                }
            }
        }

        private List<CardInfo> GetEncounterCards() {
            var packsToLoad = new List<Pack>();
            foreach (var pack in _appData.Configuration.Packs) {
                foreach (var encounterSet in pack.EncounterSets) {
                    if (_appData.Game.IsEncounterSetSelected(encounterSet.Code)) {
                        packsToLoad.Add(pack);
                        break;
                    }
                }
            }

            var allLocalCards = _localCardsService.LoadLocalCards();

            var cards = new List<CardInfo>();
            foreach (var pack in packsToLoad) {
                var arkhamDbCards = _arkhamDbService.GetCardsInPack(pack.Code);

                foreach (var arkhamDbCard in arkhamDbCards) {
                    if (!_appData.Game.IsEncounterSetSelected(arkhamDbCard.Encounter_Code)) {
                        continue;
                    }

                    // Look for corresponding local card and grab its image. Remove it from the list to avoid duplicates
                    FindCardImageSource(arkhamDbCard, allLocalCards, removeLocalCard: true);

                    var newCard = new CardInfo(arkhamDbCard, 1, isPlayerCard: false);
                    _cardImageService.LoadImage(newCard);
                    cards.Add(newCard);
                    if (!string.IsNullOrEmpty(arkhamDbCard.BackImageSrc)) {
                        var newCardBack = new CardInfo(arkhamDbCard, 1, isPlayerCard: false, cardBack: true);
                        _cardImageService.LoadImage(newCardBack);
                        newCard.FlipSideCard = newCardBack;
                        newCardBack.FlipSideCard = newCard;
                        cards.Add(newCardBack);
                    }
                }
            }

            var localCards = _localCardsService.LoadLocalCardsFromPacks(_appData.Game.LocalPacks);
            foreach (var localCard in localCards) {
                var newLocalCard = new CardInfo(localCard, false);
                _cardImageService.LoadImage(newLocalCard);
                cards.Add(newLocalCard);

                if (localCard.HasBack) {
                    var newLocalCardBack = new CardInfo(localCard, true);
                    _cardImageService.LoadImage(newLocalCardBack);
                    cards.Add(newLocalCardBack);
                }
            }

            return cards;
        }

        private static void FindCardImageSource(ArkhamDbCard arkhamDbCard, List<LocalManifestCard> localCards, bool removeLocalCard = false) {
            FixArkhamDbCardImageSource(arkhamDbCard);

            var localCard = localCards.FirstOrDefault(c => c.ArkhamDbId == arkhamDbCard.Code);
            if (localCard != null) {
                arkhamDbCard.ImageSrc = localCard.FilePath;
                if (localCard.HasBack) {
                    arkhamDbCard.BackImageSrc = localCard.BackFilePath;
                }
                if (removeLocalCard) {
                    localCards.Remove(localCard);
                }
            }
        }

        private static void FixArkhamDbCardImageSource(ArkhamDbCard arkhamDbCard) {
            string arkhamDbPrefix = "https://arkhamdb.com/";
            if (!string.IsNullOrEmpty(arkhamDbCard.ImageSrc) && !arkhamDbCard.ImageSrc.StartsWith(arkhamDbPrefix)) {
                arkhamDbCard.ImageSrc = arkhamDbPrefix + arkhamDbCard.ImageSrc;
            }
            if (!string.IsNullOrEmpty(arkhamDbCard.BackImageSrc) && !arkhamDbCard.BackImageSrc.StartsWith(arkhamDbPrefix)) {
                arkhamDbCard.BackImageSrc = arkhamDbPrefix + arkhamDbCard.BackImageSrc;
            }
        }
    }
}
