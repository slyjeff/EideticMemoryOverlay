using ArkhamOverlay.Data;
using System.Windows.Media;

namespace ArkhamOverlay.CardButtons {
    public abstract class CardImageButton : Button {
        public CardImageButton(CardTemplate cardTemplate, bool isToggled) {
            CardTemplate = cardTemplate;
            Text = cardTemplate.Name;
            IsToggled = isToggled;
        }

        public CardTemplate CardTemplate { get; }

        public override ImageSource ButtonImage { get { return CardTemplate.ButtonImage; } }
    }
}
