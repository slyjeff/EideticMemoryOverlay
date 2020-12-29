using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Media.Imaging;

namespace ArkhamOverlay.Services {
    internal class ArkhamDbService {
        internal void LoadPlayer(Player player) {
            if (string.IsNullOrEmpty(player.DeckId)) {
                return;
            }

            try {
                string url = @"https://arkhamdb.com/api/public/deck/" + player.DeckId;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream)) {
                    var arkhamDbDeck = JsonConvert.DeserializeObject<ArkhamDbDeck>(reader.ReadToEnd());
                    player.Investigator = arkhamDbDeck.Investigator_Name;
                    player.InvestigatorImage = new BitmapImage(new Uri("https://arkhamdb.com/bundles/cards/" + arkhamDbDeck.Investigator_Code + ".png", UriKind.Absolute));
                    player.CardIds = from cardId in arkhamDbDeck.Slots.Keys select cardId;
                    player.OnPlayerChanged();
                }
            } catch {
                return;
            }
            return;
        }

        internal void LoadAllPlayers(Game game) {
            foreach (var player in game.Players) {
                LoadPlayer(player);
            }
        }

        internal void LoadPlayerCards(Player player) {
            if (player.CardIds == null) {
                return;
            }

            player.Loading = true;
            try {
                var cards = new List<Card>();
                foreach (var cardId in player.CardIds) {
                    var baseCardUrl = @"https://arkhamdb.com/api/public/card/";
                    HttpWebRequest cardRequest = (HttpWebRequest)WebRequest.Create(baseCardUrl + cardId);

                    using (HttpWebResponse cardRsponse = (HttpWebResponse)cardRequest.GetResponse())
                    using (Stream cardStream = cardRsponse.GetResponseStream())
                    using (StreamReader cardReader = new StreamReader(cardStream)) {
                        var arkhamDbCard = JsonConvert.DeserializeObject<ArkhamDbCard>(cardReader.ReadToEnd());
                        cards.Add(new Card {
                            Id = cardId,
                            Name = arkhamDbCard.Xp == "0" || string.IsNullOrEmpty(arkhamDbCard.Xp) ? arkhamDbCard.Name : arkhamDbCard.Name + " (" + arkhamDbCard.Xp + ")",
                            Faction = arkhamDbCard.Faction_Name,
                            ImageSource = arkhamDbCard.Imagesrc
                        });

                    }
                }
                var playerButtons = new List<IPlayerButton> { new ClearButton() };
                playerButtons.AddRange(cards.OrderBy(x => x.Name.Replace("\"", "")));
                player.PlayerButtons = playerButtons;
                player.OnPlayerCardsChanged();
            }
            finally {
                player.Loading = false;
            }
        }

        internal bool FindMissingEncounterSets(Configuration configuration) {
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
            return setsAdded;
        }

        internal bool AddPackToConfiguration(Configuration configuration, ArkhamDbPack arkhamDbPack) {
            var cards = GetCardsInPack(arkhamDbPack);
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

        internal IList<ArkhamDbCard> GetCardsInPack(ArkhamDbPack pack) {
            var cardsInPackUrl = @"https://arkhamdb.com/api/public/cards/" + pack.Code;
            HttpWebRequest cardRequest = (HttpWebRequest)WebRequest.Create(cardsInPackUrl);

            using (HttpWebResponse cardRsponse = (HttpWebResponse)cardRequest.GetResponse())
            using (Stream cardStream = cardRsponse.GetResponseStream())
            using (StreamReader cardReader = new StreamReader(cardStream)) {
                return JsonConvert.DeserializeObject<IList<ArkhamDbCard>>(cardReader.ReadToEnd());
            }
        }
    }
}
