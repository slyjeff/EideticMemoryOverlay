using ArkhamOverlay.Common.Enums;

namespace ArkhamOverlay {
    public enum CardType {
        Asset,
        Event,
        Skill,
        Scenario,
        Agenda,
        Act,
        Enemy,
        Treachery,
        Location,
        Investigator,
        Other,
    }

    public enum Faction {
        Guardian,
        Seeker,
        Rogue,
        Mystic,
        Survivor,
        Neutral,
        Other,
    }

    public enum SelectableType {
        Player,
        Scenario,
        Location,
        Encounter,
    }

    public static class SelectableTypeExtensions {
        public static SelectableType GetSelectableType(this Deck deck) {
            switch (deck) {
                case Deck.Player1:
                case Deck.Player2:
                case Deck.Player3:
                case Deck.Player4:
                    return SelectableType.Player;
                case Deck.Scenario:
                    return SelectableType.Scenario;
                case Deck.Locations:
                    return SelectableType.Location;
                case Deck.EncounterDeck:
                    return SelectableType.Encounter;
                default:
                    return SelectableType.Player;
            }
        }
    }
}
