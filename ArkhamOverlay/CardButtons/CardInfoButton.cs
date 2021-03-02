using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Data;

namespace ArkhamOverlay.CardButtons {
    public class CardInfoButton : CardImageButton {
        public CardInfoButton(CardInfo cardInfo, CardGroupId cardGroupId) : base(cardInfo, false) {
            if (cardInfo.IsPlayerCard) {
                Options.Add(new ButtonOption(ButtonOptionOperation.Add, cardGroupId, 0));
            }

            if (cardInfo.Type == CardType.Act || cardInfo.Type == CardType.Agenda) {
                Options.Add(new ButtonOption(ButtonOptionOperation.Add, cardGroupId, 0));
            }


            if (!cardInfo.IsPlayerCard && cardInfo.IsHidden) {
                Options.Add(new ButtonOption(ButtonOptionOperation.Add, CardGroupId.Player1, 0));
                Options.Add(new ButtonOption(ButtonOptionOperation.Add, CardGroupId.Player2, 0));
                Options.Add(new ButtonOption(ButtonOptionOperation.Add, CardGroupId.Player3, 0));
                Options.Add(new ButtonOption(ButtonOptionOperation.Add, CardGroupId.Player4, 0));
            }
        }
    }
}
