using ArkhamOverlay.Services;
using System;
using System.Windows.Media;

namespace ArkhamOverlay.Data {
    public class Card : ICardButton {
        public Card() {
        }

        public Card(ArkhamDbCard arkhamDbCard, bool cardBack = false, string ownerId = null) {
            OwnerId = ownerId;
            Code = arkhamDbCard.Code;
            Name = arkhamDbCard.Xp == "0" || string.IsNullOrEmpty(arkhamDbCard.Xp) ? arkhamDbCard.Name : arkhamDbCard.Name + " (" + arkhamDbCard.Xp + ")";
            Faction = GetFaction(arkhamDbCard.Faction_Name);
            Type = GetCardType(arkhamDbCard.Type_Code);
            ImageSource = cardBack ? arkhamDbCard.BackImageSrc : arkhamDbCard.ImageSrc;

            if (cardBack) {
                Name += " (Back)";
            }
        }

        public string OwnerId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Faction Faction { get; set; }
        public string ImageSource { get; set; }
        public string BackImageSource { get; set; }
        public CardType Type { get; set; }

        public Brush Background {
            get {
                switch (Type) {
                    case CardType.Agenda:
                        return new SolidColorBrush(Colors.Chocolate);
                    case CardType.Act:
                        return new SolidColorBrush(Colors.BurlyWood);
                    case CardType.Enemy:
                        return new SolidColorBrush(Colors.SlateBlue);
                    case CardType.Treachery:
                        return new SolidColorBrush(Colors.SlateGray);
                    case CardType.Player:
                        switch(Faction) {
                            case Faction.Guardian:
                                return new SolidColorBrush(Colors.DarkBlue);
                            case Faction.Seeker:
                                return new SolidColorBrush(Colors.DarkGoldenrod);
                            case Faction.Rogue:
                                return new SolidColorBrush(Colors.DarkGreen);
                            case Faction.Survivor:
                                return new SolidColorBrush(Colors.DarkRed);
                            case Faction.Mystic:
                                return new SolidColorBrush(Colors.Indigo);
                            default:
                                return new SolidColorBrush(Colors.DarkGray);
                        }
                    default:
                        return new SolidColorBrush(Colors.DarkGray);
                }
            }
        }

        public bool IsHorizontal { get { return Type == CardType.Act || Type == CardType.Agenda; } }

        public bool IsPlayerCard => Type == CardType.Player;

        private CardType GetCardType(string typeCode) {
            if(OwnerId != null
                || string.Equals(typeCode, "asset", StringComparison.OrdinalIgnoreCase)
                || string.Equals(typeCode, "event", StringComparison.OrdinalIgnoreCase)
                || string.Equals(typeCode, "skill", StringComparison.OrdinalIgnoreCase)) {
                return CardType.Player;
            } else if (Enum.TryParse(typeCode, ignoreCase: true, out CardType type)) {
                return type;
            } else {
                return CardType.Other;
            }
        }

        private Faction GetFaction(string factionCode) {
            if (Enum.TryParse(factionCode, ignoreCase: true, out Faction type)) {
                return type;
            }
            else {
                return Faction.Other;
            }
        }
    }
}
