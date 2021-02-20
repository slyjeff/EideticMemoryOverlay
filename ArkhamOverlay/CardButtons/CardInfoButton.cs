using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Data;
using System.Windows.Media;

namespace ArkhamOverlay.CardButtons {
    public class CardInfoButton : CardImageButton {
        public CardInfoButton(CardInfo cardInfo) : base(cardInfo, false) {
            if (!cardInfo.IsPlayerCard && cardInfo.IsHidden) {
                Options.Add(new ButtonOption(CardGroupId.Player1.ToString(), $"Add to <<{CardGroupId.Player1}>>'s hand"));
                Options.Add(new ButtonOption(CardGroupId.Player2.ToString(), $"Add to <<{CardGroupId.Player2}>>'s hand"));
                Options.Add(new ButtonOption(CardGroupId.Player3.ToString(), $"Add to <<{CardGroupId.Player3}>>'s hand"));
                Options.Add(new ButtonOption(CardGroupId.Player4.ToString(), $"Add to <<{CardGroupId.Player4}>>'s hand"));
            }
        }

        public override ImageSource ButtonImage { get { return CardInfo.ButtonImage; } }
    }
}
