namespace EideticMemoryOverlay.PluginApi.Buttons {
    public class CardButton : CardImageButton, ICard {
        public CardButton(CardImageButton button) : base(button.CardInfo, button.IsToggled) {
        }
    }
}
