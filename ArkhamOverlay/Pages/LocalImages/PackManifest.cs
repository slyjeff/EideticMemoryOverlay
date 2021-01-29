using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ArkhamOverlay.Pages.LocalImages {
    public class PackManifest {
        public PackManifest() {
            Cards = new List<CardManifest>();   
        }

        public PackManifest(LocalPack pack) {
            Name = pack.Name;
            Cards = new List<CardManifest>();
            foreach (var card in pack.Cards) {
                Cards.Add(new CardManifest {
                    FilePath = card.FilePath,
                    Name = card.Name,
                    CardType = card.CardType,
                    HasBack = card.HasBack
                });
            }
        }

        public string Name { get; set; }

        public IList<CardManifest> Cards { get; set; }
    }

    public class CardManifest {
        public string FilePath { get; set; }
        public string Name { get; set; }
        public bool HasBack { get; set; }
        public string CardType { get; set; }
    }
}
