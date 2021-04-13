using Emo.Data;

namespace Emo.CardButtons {
    public class CardButton : CardImageButton, ICard {
        public CardButton(CardImageButton button) : base(button.CardInfo, button.IsToggled) {
        }
    }
}
