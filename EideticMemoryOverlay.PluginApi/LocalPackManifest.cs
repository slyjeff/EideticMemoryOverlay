using System.Collections.Generic;

namespace EideticMemoryOverlay.PluginApi {
    public class LocalPackManifest {
        public LocalPackManifest() {
            Cards = new List<LocalCard>();
        }

        public string Name { get; set; }

        public IList<LocalCard> Cards { get; set; }
    }


    public static class LocalCardExtensions {
        public static void CopyTo(this ILocalCard sourceCard, ILocalCard destinationCard) {
            destinationCard.FilePath = sourceCard.FilePath;
            destinationCard.Name = sourceCard.Name;
            destinationCard.HasBack = sourceCard.HasBack;
            destinationCard.CardType = sourceCard.CardType;
            destinationCard.ArkhamDbId = sourceCard.ArkhamDbId;
        }
    }
}
