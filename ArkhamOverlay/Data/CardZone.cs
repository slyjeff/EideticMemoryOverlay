using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ArkhamOverlay.Data {
    public enum CardZoneLocation { Top, Bottom }

    /// <summary>
    /// Represents a physical location (hand, act/agenda bar) and contains a list of instances of cards
    /// </summary>
    public class CardZone {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        public CardZone(string name, CardZoneLocation location) {
            Name = name;
            Buttons = new ObservableCollection<CardButton>();
            Location = location;
        }

        public string Name { get; }
        public CardZoneLocation Location { get; }
        public CardGroupId CardGroupId { get; set; }

        public ObservableCollection<CardButton> Buttons { get; }

        public IEnumerable<ICard> Cards { get => Buttons; }

        /// <summary>
        /// Create a card and add it to the first zone
        /// </summary>
        /// <param name="button">Button that initiated this create- contains card info and toggle state</param>
        /// <returns>The newly created button</returns>
        public CardButton CreateCardButton(CardImageButton button) {
            var buttonToReplace = Buttons.FirstOrDefault(x => x.CardInfo == button.CardInfo.FlipSideCard);
            if (buttonToReplace != null) {
                return ReplaceButton(button, buttonToReplace);
            }
            return AddButton(button);
        }

        /// <summary>
        /// Replace an existing button with a new  button
        /// </summary>
        /// <param name="button">Button that intiated this create- contains card info and toggle state</param>
        /// <param name="buttonToReplace">The button to replace</param>
        /// <returns>The newly created button</returns>
        private CardButton ReplaceButton(CardImageButton button, CardButton buttonToReplace) {
            var index = Buttons.IndexOf(buttonToReplace);
            var newButton = new CardButton(button);
            Buttons[index] = newButton;
            PublishButtonInfoChanged(newButton, ChangeAction.Update);
            return newButton;
        }

        /// <summary>
        /// Create a new button and add it to the end of the list
        /// </summary>
        /// <param name="button">Button that intiated this create- contains card info and toggle state</param>
        /// <param name="buttonToReplace">The button to replace</param>
        /// <returns>The newly created button</returns>
        private CardButton AddButton(CardImageButton button) {
            var existingCopyCount = Buttons.Count(x => x.CardInfo == button.CardInfo);

            //don't add more than one copy unless it's a player card
            if (!button.CardInfo.IsPlayerCard && existingCopyCount > 0) {
                return null;
            }

            //if there's an act and this is an agenda, always add it to the left
            var index = Buttons.Count();
            if (button.CardInfo.Type == CardType.Agenda && Buttons.Any(x => x.CardInfo.Type == CardType.Act)) {
                index = Buttons.IndexOf(Buttons.First(x => x.CardInfo.Type == CardType.Act));
            }

            var newButton = new CardButton(button);
            Buttons.Insert(index, newButton);
            PublishButtonInfoChanged(newButton, ChangeAction.Add);
            return newButton;
        }

        /// <summary>
        /// If this button is in the list, remove it
        /// </summary>
        /// <param name="button">Button to remove</param>
        internal void RemoveButton(CardButton button) {
            var indexOfRemovedCard = Buttons.IndexOf(button);
            if (indexOfRemovedCard == -1) {
                return;
            }

            Buttons.Remove(button);
            PublishButtonRemoved(indexOfRemovedCard); 
        }

        private void PublishButtonInfoChanged(CardButton button, ChangeAction action) {
            var index = Buttons.IndexOf(button);
            var isImageAvailable = button?.CardInfo.ButtonImageAsBytes != null;

            _eventBus.PublishButtonInfoChanged(CardGroupId, ButtonMode.Zone, index, button.Text, button.CardInfo.ImageId, button.IsToggled, isImageAvailable, action, button.Options);
        }

        private void PublishButtonRemoved(int index) {
            _eventBus.PublishButtonRemoved(CardGroupId, ButtonMode.Zone, index);
        }
    }
}