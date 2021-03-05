using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Events;
using PageController;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ArkhamOverlay.Data {
    public enum CardZoneLocation { Top, Bottom }

    /// <summary>
    /// Represents a physical location (hand, act/agenda bar) and contains a list of instances of cards
    /// </summary>
    public class CardZone : ViewModel {
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
        
        public Visibility IsVisible { get { return Buttons.Any() ? Visibility.Visible : Visibility.Collapsed; } }

        public IEnumerable<ICard> Cards { get => Buttons; }
        public int ZoneIndex { get; set; }

        /// <summary>
        /// Create a card and add it to the first zone
        /// </summary>
        /// <param name="button">Button that initiated this create- contains card info and toggle state</param>
        /// <param name="options">Options this button should offer on a right click</param>
        /// <returns>The newly created button</returns>
        public void CreateCardButton(CardImageButton button, IEnumerable<ButtonOption> options) {
            var buttonToReplace = Buttons.FirstOrDefault(x => x.CardInfo == button.CardInfo.FlipSideCard);
            if (buttonToReplace != null) {
                ReplaceButton(button, buttonToReplace, options);
            }
            AddButton(button, options);
            NotifyPropertyChanged(nameof(IsVisible));
        }

        /// <summary>
        /// Replace an existing button with a new  button
        /// </summary>
        /// <param name="button">Button that intiated this create- contains card info and toggle state</param>
        /// <param name="buttonToReplace">The button to replace</param>
        /// <param name="options">Options this button should offer on a right click</param>
        private void ReplaceButton(CardImageButton button, CardButton buttonToReplace, IEnumerable<ButtonOption> options) {
            var index = Buttons.IndexOf(buttonToReplace);
            var newButton = new CardButton(button);

            foreach (var option in options) {
                newButton.Options.Add(option);
            }

            Buttons[index] = newButton;
            PublishButtonInfoChanged(newButton, ChangeAction.Update);
        }

        /// <summary>
        /// Create a new button and add it to the end of the list
        /// </summary>
        /// <param name="button">Button that intiated this create- contains card info and toggle state</param>
        /// <param name="options">Options this button should offer on a right click</param>
        private void AddButton(CardImageButton button, IEnumerable<ButtonOption> options) {
            var existingCopyCount = Buttons.Count(x => x.CardInfo == button.CardInfo);

            //if there's an act and this is an agenda, always add it to the left
            var index = Buttons.Count();
            if (button.CardInfo.Type == CardType.Agenda && Buttons.Any(x => x.CardInfo.Type == CardType.Act)) {
                index = Buttons.IndexOf(Buttons.First(x => x.CardInfo.Type == CardType.Act));
            }

            var newButton = new CardButton(button);
            
            foreach (var option in options) {
                newButton.Options.Add(option);
            }

            Buttons.Insert(index, newButton);
            PublishButtonInfoChanged(newButton, ChangeAction.Add);
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
            NotifyPropertyChanged(nameof(IsVisible));
        }

        private void PublishButtonInfoChanged(CardButton button, ChangeAction action) {
            var index = Buttons.IndexOf(button);
            var isImageAvailable = button?.CardInfo.ButtonImageAsBytes != null;

            _eventBus.PublishButtonInfoChanged(CardGroupId, ButtonMode.Zone, ZoneIndex, index, button.Text, button.CardInfo.ImageId, button.IsToggled, isImageAvailable, action, button.Options);
        }

        private void PublishButtonRemoved(int index) {
            _eventBus.PublishButtonRemoved(CardGroupId, ButtonMode.Zone, ZoneIndex, index);
        }
    }
}