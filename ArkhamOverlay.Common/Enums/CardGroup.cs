namespace ArkhamOverlay.Common.Enums {
    public enum CardGroup {
        Player1,
        Player2,
        Player3,
        Player4,
        Scenario,
        Locations,
        EncounterDeck
    }

    public static class DeckExtensions {
        public static CardGroup AsCardGroup(this string deck) {
            switch (deck) {
                case "player1": return CardGroup.Player1;
                case "player2": return CardGroup.Player2;
                case "player3": return CardGroup.Player3;
                case "player4": return CardGroup.Player4;
                case "scenario": return CardGroup.Scenario;
                case "locations": return CardGroup.Locations;
                case "encounterDeck": return CardGroup.EncounterDeck;
                default: return CardGroup.Player1;
            }
        }
    }
}
