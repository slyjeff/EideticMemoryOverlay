using System.Drawing;

namespace ArkhamOverlay.TcpUtils {
    public enum CardButtonType { Action, Scenario, Agenda, Act, Location, Enemy, Treachery, Asset, Skill, Event, Unknown }

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
                case CardButtonType.Asset:
                    return Color.DarkBlue;
                case CardButtonType.Skill:
                    return Color.DarkGoldenrod;
                case CardButtonType.Event:
                    return Color.DarkGreen;
                default:
                    return Color.DarkGray;
            }

        }
    }
}
