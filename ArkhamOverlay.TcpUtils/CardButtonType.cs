using System.Drawing;

namespace ArkhamOverlay.TcpUtils {
    public enum CardButtonType { Action, Scenario, Agenda, Act, Location, Enemy, Treachery, Guardian, Seeker, Rogue, Survivor, Mystic, Unknown }

    public static class CardButtonTypeExtensions {
        public static Color AsColor(this CardButtonType cardButtonType) {
            switch (cardButtonType) {
                case CardButtonType.Action:
                    return Color.Black;
                case CardButtonType.Agenda:
                    return Color.Chocolate;
                case CardButtonType.Act:
                    return Color.BurlyWood;
                case CardButtonType.Enemy:
                    return Color.SlateBlue;
                case CardButtonType.Treachery:
                    return Color.SlateGray;
                case CardButtonType.Guardian:
                    return Color.DarkBlue;
                case CardButtonType.Seeker:
                    return Color.DarkGoldenrod;
                case CardButtonType.Rogue:
                    return Color.DarkGreen;
                case CardButtonType.Survivor:
                    return Color.DarkRed;
                case CardButtonType.Mystic:
                    return Color.Indigo;
                default:
                    return Color.DarkGray;
            }

        }
    }
}
