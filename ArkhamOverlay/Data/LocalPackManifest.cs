using System.Collections.Generic;

namespace ArkhamOverlay.Data {
    public class LocalPackManifest {
        public LocalPackManifest() {
            Cards = new List<LocalManifestCard>();   
        }

        public string Name { get; set; }

        public IList<LocalManifestCard> Cards { get; set; }
    }

    public interface ILocalCard {
        string FilePath { get; set; }
        string Name { get; set; }
        bool HasBack { get; set; }
        string CardType { get; set; }
        string ArkhamDbId { get; set; }
    }

    public class LocalManifestCard : ILocalCard {
        public string FilePath { get; set; }
        public string Name { get; set; }
        public bool HasBack { get; set; }
        public string CardType { get; set; }
        public string ArkhamDbId { get; set; }
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
