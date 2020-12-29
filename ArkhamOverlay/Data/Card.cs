using ArkhamOverlay.Services;
using System.Windows.Media;

namespace ArkhamOverlay.Data {
    public class Card : ICardButton {
        public Card() {
        }

        public Card(ArkhamDbCard arkhamDbCard, bool cardBack = false) {
            Code = arkhamDbCard.Code;
            Name = arkhamDbCard.Xp == "0" || string.IsNullOrEmpty(arkhamDbCard.Xp) ? arkhamDbCard.Name : arkhamDbCard.Name + " (" + arkhamDbCard.Xp + ")";
            Faction = arkhamDbCard.Faction_Name;
            TypeCode = arkhamDbCard.Type_Code;
            ImageSource = cardBack ? arkhamDbCard.BackImageSrc : arkhamDbCard.ImageSrc;

            if (cardBack) {
                Name += " (Back)";
            }
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Faction { get; set; }
        public string ImageSource { get; set; }
        public string BackImageSource { get; set; }
        public string TypeCode { get; set; }

        public Brush Background {
            get {
                if (Faction == "Guardian") {
                    return new SolidColorBrush(Colors.DarkBlue);
                }

                if (Faction == "Seeker") {
                    return new SolidColorBrush(Colors.DarkGoldenrod);
                }

                if (Faction == "Rogue") {
                    return new SolidColorBrush(Colors.DarkGreen);
                }

                if (Faction == "Survivor") {
                    return new SolidColorBrush(Colors.DarkRed);
                }

                if (Faction == "Mystic") {
                    return new SolidColorBrush(Colors.Indigo);
                }

                if (TypeCode == "agenda") {
                    return new SolidColorBrush(Colors.Chocolate);
                }

                if (TypeCode == "act") {
                    return new SolidColorBrush(Colors.BurlyWood);
                }

                if (TypeCode == "enemy") {
                    return new SolidColorBrush(Colors.SlateBlue);
                }

                if (TypeCode == "treachery") {
                    return new SolidColorBrush(Colors.SlateGray);
                }

                return new SolidColorBrush(Colors.DarkGray);
            }
        }

        public bool IsHorizontal { get { return TypeCode == "act" || TypeCode == "agenda"; } }

        public bool IsPlayerCard { get { return TypeCode == "asset" || TypeCode == "event" || TypeCode == "skill"; } }
    }
}
