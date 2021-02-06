namespace ArkhamOverlay.Common.Enums {
    public enum Deck {
        Player1,
        Player2,
        Player3,
        Player4,
        Scenario,
        Locations,
        EncounterDeck
    }

    public static class DeckExtensions {
        public static Deck AsDeck(this string deck) {
            switch (deck) {
                case "player1": return Deck.Player1;
                case "player2": return Deck.Player2;
                case "player3": return Deck.Player3;
                case "player4": return Deck.Player4;
                case "scenario": return Deck.Scenario;
                case "locations": return Deck.Locations;
                case "encounterDeck": return Deck.EncounterDeck;
                default: return Deck.Player1;
            }
        }
    }
}
