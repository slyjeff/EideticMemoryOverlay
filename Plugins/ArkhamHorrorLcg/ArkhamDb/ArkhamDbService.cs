using EideticMemoryOverlay.PluginApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace ArkhamHorrorLcg.ArkhamDb {
    public class ArkhamDbService {
        private readonly ILoggingService _logger;

        public ArkhamDbService(ILoggingService loggingService) {
            _logger = loggingService;
        }

        public ArkhamDbDeck GetPlayerDeck(string deckId) {
            if(string.IsNullOrEmpty(deckId)) {
                return null;
            }

            _logger.LogMessage($"Loading deck {deckId}");

            string url = @"https://arkhamdb.com/api/public/deck/" + deckId;

            var request = (HttpWebRequest)WebRequest.Create(url);

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream)) {
                return JsonConvert.DeserializeObject<ArkhamDbDeck>(reader.ReadToEnd());
            }
        }

        public IList<ArkhamDbCard> GetCardsInPack(string packCode) {
            var cardsInPackUrl = @"https://arkhamdb.com/api/public/cards/" + packCode;
            HttpWebRequest cardRequest = (HttpWebRequest)WebRequest.Create(cardsInPackUrl);

            using (HttpWebResponse cardRsponse = (HttpWebResponse)cardRequest.GetResponse())
            using (Stream cardStream = cardRsponse.GetResponseStream())
            using (StreamReader cardReader = new StreamReader(cardStream)) {
                return JsonConvert.DeserializeObject<IList<ArkhamDbCard>>(cardReader.ReadToEnd());
            }
        }

        public ArkhamDbCard GetCard(string cardCode) {
            return LocalCardCache.Instance.GetCard(cardCode) ?? GetCardFromArkhamDb(cardCode);
        }

        internal IList<ArkhamDbPack> GetAllPacks() {
            var packsUrl = @"https://arkhamdb.com/api/public/packs/";
            HttpWebRequest cardRequest = (HttpWebRequest)WebRequest.Create(packsUrl);

            _logger.LogMessage("Fetching all packs");

            using (HttpWebResponse cardRsponse = (HttpWebResponse)cardRequest.GetResponse())
            using (Stream cardStream = cardRsponse.GetResponseStream())
            using (StreamReader cardReader = new StreamReader(cardStream)) {
                return JsonConvert.DeserializeObject<List<ArkhamDbPack>>(cardReader.ReadToEnd());
            }
        }

        private ArkhamDbCard GetCardFromArkhamDb(string cardCode) {
            var baseCardUrl = @"https://arkhamdb.com/api/public/card/";
            HttpWebRequest cardRequest = (HttpWebRequest)WebRequest.Create(baseCardUrl + cardCode);
            try {
                using (HttpWebResponse cardRsponse = (HttpWebResponse)cardRequest.GetResponse())
                using (Stream cardStream = cardRsponse.GetResponseStream())
                using (StreamReader cardReader = new StreamReader(cardStream)) {
                    return JsonConvert.DeserializeObject<ArkhamDbCard>(cardReader.ReadToEnd());
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
    }
}
