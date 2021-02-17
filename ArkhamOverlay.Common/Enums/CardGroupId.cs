using System.Collections.Generic;

namespace ArkhamOverlay.Common.Enums {
    public enum CardGroupId {
        Player1,
        Player2,
        Player3,
        Player4,
        Scenario,
        Locations,
        EncounterDeck
    }

    public static class CardGroupIdExtensions {
        public static CardGroupId AsCardGroupId(this string deck) {
            switch (deck) {
                case "player1": return CardGroupId.Player1;
                case "player2": return CardGroupId.Player2;
                case "player3": return CardGroupId.Player3;
                case "player4": return CardGroupId.Player4;
                case "scenario": return CardGroupId.Scenario;
                case "locations": return CardGroupId.Locations;
                case "encounterDeck": return CardGroupId.EncounterDeck;
                default: return CardGroupId.Player1;
            }
        }
    }
}
