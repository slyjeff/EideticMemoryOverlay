using System.Collections.Generic;

namespace ArkhamOverlay.Services {
    public class ArkhamDbCard {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Faction_Name { get; set; }
        public string ImageSrc { get; set; }
        public string BackImageSrc { get; set; }
        public string Xp { get; set; }
        public string Encounter_Code { get; set; }
        public string Encounter_Name { get; set; }
        public string Type_Code { get; set; }
        public int Health { get; set; }
        public int Sanity { get; set; }
    }

    public class ArkhamDbFullCard : ArkhamDbCard {
        public List<BondedCard> Bonded_Cards;
    }

    public class BondedCard {
        public int Count;
        public string Code;
    }
}
