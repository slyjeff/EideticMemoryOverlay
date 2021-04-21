using Emo.Common.Enums;

namespace Emo {
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

    public enum CardGroupType {
        Player,
        Scenario,
        Location,
        Encounter,
    }

    public static class SelectableTypeExtensions {
        public static CardGroupType GetSelectableType(this CardGroupId deck) {
            switch (deck) {
                case CardGroupId.Player1:
                case CardGroupId.Player2:
                case CardGroupId.Player3:
                case CardGroupId.Player4:
                    return CardGroupType.Player;
                case CardGroupId.Scenario:
                    return CardGroupType.Scenario;
                case CardGroupId.Locations:
                    return CardGroupType.Location;
                case CardGroupId.EncounterDeck:
                    return CardGroupType.Encounter;
                default:
                    return CardGroupType.Player;
            }
        }
    }
}
