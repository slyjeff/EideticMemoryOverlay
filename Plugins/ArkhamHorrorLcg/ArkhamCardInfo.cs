using ArkhamHorrorLcg.ArkhamDb;
using EideticMemoryOverlay.PluginApi;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace ArkhamHorrorLcg {
    /// <summary>
    /// All information about a card, including images from either arkham db or local stored
    /// </summary>
    internal class ArkhamCardInfo : CardInfo {
        public ArkhamCardInfo() {
        }

        internal ArkhamCardInfo(ArkhamDbCard arkhamDbCard, int count, bool isPlayerCard, bool cardBack = false, bool isBonded = false) 
            : base(FormatNameWithXp(arkhamDbCard), arkhamDbCard.Code, isPlayerCard, count, cardBack, cardBack ? arkhamDbCard.BackImageSrc : arkhamDbCard.ImageSrc) {
            NameWithoutXp = arkhamDbCard.Name;
            Xp = arkhamDbCard.Xp == null ? 0 : int.Parse(arkhamDbCard.Xp);
            Faction = GetFaction(arkhamDbCard.Faction_Name);
            Type = GetCardType(arkhamDbCard.Type_Code);
            IsBonded = isBonded;
            IsHidden = !string.IsNullOrEmpty(arkhamDbCard.Text) && arkhamDbCard.Text.Contains(" Hidden.");
        }

        private static string FormatNameWithXp(ArkhamDbCard arkhamDbCard) {
            return arkhamDbCard.Xp == "0" || string.IsNullOrEmpty(arkhamDbCard.Xp) ? arkhamDbCard.Name : arkhamDbCard.Name + " (" + arkhamDbCard.Xp + ")";
        }

        internal ArkhamCardInfo(ArkhamLocalCard localCard, bool cardBack)
            : base(localCard.Name, GetLocalCardCode(localCard), false, 1, cardBack, cardBack ? localCard.BackFilePath : localCard.FilePath) {
            NameWithoutXp = localCard.Name;
            Xp = 0;
            Faction = Faction.Other;
            Type = (CardType)Enum.Parse(typeof(CardType), localCard.CardType);
            IsBonded = false;
            IsHidden = false;
        }

        private static string GetLocalCardCode(ArkhamLocalCard localCard) {
            return string.IsNullOrEmpty(localCard.ArkhamDbId) ? Path.GetFileName(localCard.FilePath) : localCard.ArkhamDbId;
        }

        public override bool IsHorizontal { get { return Type == CardType.Act || Type == CardType.Agenda; } }

        //don't sort scenario cards- easier to find when acts/agendas are in order
        public override CardInfoSort SortBy { get { return Type == CardType.Scenario ? CardInfoSort.Natural : CardInfoSort.Alphabetical; } }

        public override Point GetCropStartingPoint() {
            switch (Type) {
                case CardType.Scenario:
                    return new Point(40, 60);
                case CardType.Agenda:
                    return new Point(10, 40);
                case CardType.Investigator:
                    return new Point(10, 50);
                case CardType.Act:
                    return new Point(190, 40);
                case CardType.Location:
                    return new Point(40, 40);
                case CardType.Enemy:
                    return new Point(40, 190);
                case CardType.Treachery:
                    return new Point(40, 0);
                case CardType.Asset:
                    return new Point(40, 40);
                case CardType.Event:
                    return new Point(40, 0);
                case CardType.Skill:
                    return new Point(40, 40);
                default:
                    return new Point(40, 40);
            }
        }

        internal string NameWithoutXp { get; }
        internal int Xp { get; }
        internal Faction Faction { get; set; }
        internal bool IsBonded { get; }
        internal bool IsHidden { get; }

        internal CardType Type { get; }

        internal CardType ImageCardType { get { return Type; } }

        private CardType GetCardType(string typeCode) {
            if (Enum.TryParse(typeCode, ignoreCase: true, out CardType type)) {
                return type;
            } else {
                return CardType.Other;
            }
        }

        private Faction GetFaction(string factionCode) {
            if (Enum.TryParse(factionCode, ignoreCase: true, out Faction type)) {
                return type;
            } else {
                return Faction.Other;
            }
        }

        public override string DeckListName {
            get {
                var name = NameWithoutXp;
                for (var x = 0; x < Xp; x++) {
                    name += "•";
                }

                return Count + "x " + name;
            }
        }

        public override Brush DeckListColor { 
            get {
                switch (Faction) {
                    case Faction.Guardian: return new SolidColorBrush(Colors.DarkBlue);
                    case Faction.Seeker: return new SolidColorBrush(Colors.DarkGoldenrod);
                    case Faction.Mystic: return new SolidColorBrush(Colors.Purple);
                    case Faction.Rogue: return new SolidColorBrush(Colors.DarkGreen);
                    case Faction.Survivor: return new SolidColorBrush(Colors.DarkRed);
                    default: return new SolidColorBrush(Colors.Black);
                }
            } 
        }
    }
}
