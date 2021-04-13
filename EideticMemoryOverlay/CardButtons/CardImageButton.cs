using Emo.Data;
using System.Windows.Media;

namespace Emo.CardButtons {
    public abstract class CardImageButton : Button {
        public CardImageButton(CardInfo cardInfo, bool isToggled) {
            CardInfo = cardInfo;
            Text = cardInfo.Name;
            IsToggled = isToggled;
        }

        public CardInfo CardInfo { get; }

        public override ImageSource ButtonImage { get { return CardInfo.ButtonImage; } }
    }
}
