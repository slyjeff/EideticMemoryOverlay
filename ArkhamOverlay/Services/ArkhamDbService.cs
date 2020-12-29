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
                request.AutomaticDecompression = DecompressionMethods.GZip;

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
                    cardRequest.AutomaticDecompression = DecompressionMethods.GZip;

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
                player.Cards = cards.OrderBy(x => x.Name.Replace("\"", "")).ToList();
                player.OnPlayerCardsChanged();
            } finally {
                player.Loading = false;
            }
        }
    }
}
