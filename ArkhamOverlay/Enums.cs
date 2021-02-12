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
        public static SelectableType GetSelectableType(this CardGroup deck) {
            switch (deck) {
                case CardGroup.Player1:
                case CardGroup.Player2:
                case CardGroup.Player3:
                case CardGroup.Player4:
                    return SelectableType.Player;
                case CardGroup.Scenario:
                    return SelectableType.Scenario;
                case CardGroup.Locations:
                    return SelectableType.Location;
                case CardGroup.EncounterDeck:
                    return SelectableType.Encounter;
                default:
                    return SelectableType.Player;
            }
        }
    }
}
