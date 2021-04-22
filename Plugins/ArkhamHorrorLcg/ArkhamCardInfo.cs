using ArkhamHorrorLcg.ArkhamDb;
using EideticMemoryOverlay.PluginApi;
using System;
using System.IO;

namespace ArkhamHorrorLcg {
    /// <summary>
    /// All information about a card, including images from either arkham db or local stored
    /// </summary>
    internal class ArkhamCardInfo : CardInfo {
        public ArkhamCardInfo() {
        }

        internal ArkhamCardInfo(ArkhamDbCard arkhamDbCard, int count, bool isPlayerCard, bool cardBack = false, bool isBonded = false) 
            : base(FormatNameWithXp(arkhamDbCard), arkhamDbCard.Code, count, cardBack, cardBack ? arkhamDbCard.BackImageSrc : arkhamDbCard.ImageSrc) {
            NameWithoutXp = arkhamDbCard.Name;
            Xp = arkhamDbCard.Xp == null ? 0 : int.Parse(arkhamDbCard.Xp);
            Faction = GetFaction(arkhamDbCard.Faction_Name);
            Type = GetCardType(arkhamDbCard.Type_Code);
            IsPlayerCard = isPlayerCard;
            IsBonded = isBonded;
            IsHidden = !string.IsNullOrEmpty(arkhamDbCard.Text) && arkhamDbCard.Text.Contains(" Hidden.");
        }

        private static string FormatNameWithXp(ArkhamDbCard arkhamDbCard) {
            return arkhamDbCard.Xp == "0" || string.IsNullOrEmpty(arkhamDbCard.Xp) ? arkhamDbCard.Name : arkhamDbCard.Name + " (" + arkhamDbCard.Xp + ")";
        }

        internal ArkhamCardInfo(ArkhamLocalCard localCard, bool cardBack)
            : base(localCard.Name, GetLocalCardCode(localCard), 1, cardBack, cardBack ? localCard.BackFilePath : localCard.FilePath) {
            NameWithoutXp = localCard.Name;
            Xp = 0;
            Faction = Faction.Other;
            Type = (CardType)Enum.Parse(typeof(CardType), localCard.CardType);
            IsPlayerCard = false;
            IsBonded = false;
            IsHidden = false;
        }

        private static string GetLocalCardCode(ArkhamLocalCard localCard) {
            return string.IsNullOrEmpty(localCard.ArkhamDbId) ? Path.GetFileName(localCard.FilePath) : localCard.ArkhamDbId;
        }

        public override bool IsHorizontal { get { return Type == CardType.Act || Type == CardType.Agenda; } }

        internal string NameWithoutXp { get; }
        internal int Xp { get; }
        internal Faction Faction { get; set; }
        internal bool IsBonded { get; }
        internal bool IsHidden { get; }

        internal CardType Type { get; }

        internal CardType ImageCardType { get { return Type; } }

        internal bool IsPlayerCard { get; private set; }

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
    }
}
