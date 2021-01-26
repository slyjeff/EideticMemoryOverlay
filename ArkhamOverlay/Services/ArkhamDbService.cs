using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Data;
using ArkhamOverlay.Pages.Main;
using ArkhamOverlay.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace ArkhamOverlay.Services {
    public class ArkhamDbService {
        internal void LoadPlayer(Player player) {
            if (string.IsNullOrEmpty(player.DeckId)) {
                return;
            }

            string url = @"https://arkhamdb.com/api/public/deck/" + player.DeckId;

            var request = (HttpWebRequest)WebRequest.Create(url);

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream)) {
                var arkhamDbDeck = JsonConvert.DeserializeObject<ArkhamDbDeck>(reader.ReadToEnd());
                player.SelectableCards.Name = arkhamDbDeck.Investigator_Name;
                player.InvestigatorCode = arkhamDbDeck.Investigator_Code;
                player.Slots = arkhamDbDeck.Slots;
                player.LoadImage("https://arkhamdb.com/bundles/cards/" + arkhamDbDeck.Investigator_Code + ".png");
            }

            var investigatorUrl = @"https://arkhamdb.com/api/public/card/" + player.InvestigatorCode;
            var investigatorRequest = (HttpWebRequest)WebRequest.Create(investigatorUrl);
            using (var response = (HttpWebResponse)investigatorRequest.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream)) {
                var arkhamDbCard = JsonConvert.DeserializeObject<ArkhamDbCard>(reader.ReadToEnd());

                if (Enum.TryParse(arkhamDbCard.Faction_Name, ignoreCase: true, out Faction faction)) {
                    player.Faction = faction;
                }
            }


            player.OnPlayerChanged();

            return;
        }

        internal void LoadPlayerCards(Player player) {
            if (player.Slots == null) {
                return;
            }

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
                        cards.Add(new Card(arkhamDbCard, slot.Value, true));
                    }
                }
                player.SelectableCards.LoadCards(cards);
            }
            finally {
                player.SelectableCards.Loading = false;
            }
        }

        internal void FindMissingEncounterSets(Configuration configuration) {
            var packsUrl = @"https://arkhamdb.com/api/public/packs/";
            HttpWebRequest cardRequest = (HttpWebRequest)WebRequest.Create(packsUrl);

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

            return true;
        }

        internal void LoadEncounterCards(AppData mainViewModel) {
            try {
                mainViewModel.Game.ScenarioCards.Loading = true;
                mainViewModel.Game.LocationCards.Loading = true;
                mainViewModel.Game.EncounterDeckCards.Loading = true;

                var packsToLoad = new List<Pack>();
                foreach (var pack in mainViewModel.Configuration.Packs) {
                    foreach (var encounterSet in pack.EncounterSets) {
                        if (mainViewModel.Game.IsEncounterSetSelected(encounterSet.Code)) {
                            packsToLoad.Add(pack);
                            break;
                        }
                    }
                }

                var scenarioCards = new List<Card>();
                var agendas = new List<Card>();
                var acts = new List<Card>();
                var locations = new List<Card>();
                var treacheries = new List<Card>();
                var enemies = new List<Card>();

                foreach (var pack in packsToLoad) {
                    var arkhamDbCards = GetCardsInPack(pack.Code);
                    var cards = new List<Card>();

                    foreach (var arkhamDbCard in arkhamDbCards) {
                        if (!mainViewModel.Game.IsEncounterSetSelected(arkhamDbCard.Encounter_Code)) {
                            continue;
                        }

                        var newCard = new Card(arkhamDbCard, 1, false);
                        cards.Add(newCard);
                        if (!string.IsNullOrEmpty(arkhamDbCard.BackImageSrc)) {
                            var newCardBack = new Card(arkhamDbCard, 1, false, true);
                            newCard.FlipSideCard = newCardBack;
                            newCardBack.FlipSideCard = newCard;
                            cards.Add(newCardBack);
                        }
                    }

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
                }

                scenarioCards.AddRange(agendas);
                scenarioCards.AddRange(acts);
                mainViewModel.Game.ScenarioCards.LoadCards(scenarioCards);
                mainViewModel.Game.LocationCards.LoadCards(locations);
                mainViewModel.Game.EncounterDeckCards.LoadCards(treacheries);
            } finally {
                mainViewModel.Game.ScenarioCards.Loading = false;
                mainViewModel.Game.LocationCards.Loading = false;
                mainViewModel.Game.EncounterDeckCards.Loading = false;
            }
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
