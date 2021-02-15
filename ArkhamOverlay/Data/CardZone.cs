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

        public void AddCard(CardTemplate card) {
            var cardSetButtonToReplace = Buttons.FirstOrDefault(x => x.CardTemplate == card.FlipSideCard);
            if (cardSetButtonToReplace != null) {
                var index = Buttons.IndexOf(cardSetButtonToReplace);
                var newButton = new CardButton(this, card);
                Buttons[index] = newButton;
                PublishButtonInfoChanged(newButton, ChangeAction.Update);
            } else {
                var existingCopyCount = Buttons.Count(x => x.CardTemplate == card);

                //don't add more than one copy unless it's a player card
                if (!card.IsPlayerCard && existingCopyCount > 0) {
                    return;
                }

                //if there's an act and this is an agenda, always add it to the left
                var index = Buttons.Count();
                if (card.Type == CardType.Agenda && Buttons.Any(x => x.CardTemplate.Type == CardType.Act)) {
                    index = Buttons.IndexOf(Buttons.First(x => x.CardTemplate.Type == CardType.Act));
                }

                var newButton = new CardButton(this, card);
                Buttons.Insert(index, newButton);
                PublishButtonInfoChanged(newButton, ChangeAction.Add);
            }
        }

        public void RemoveCard(CardButton cardButton) {
            var indexOfRemovedCard = Buttons.IndexOf(cardButton);
            Buttons.Remove(cardButton);
            PublishButtonRemoved(indexOfRemovedCard); 
        }

        private void PublishButtonInfoChanged(CardButton button, ChangeAction action) {
            var index = Buttons.IndexOf(button);
            var isImageAvailable = button?.CardTemplate.ButtonImageAsBytes != null;

            _eventBus.PublishButtonInfoChanged(CardGroupId, ButtonMode.Zone, index, button.Text, button.IsToggled, isImageAvailable, action);
        }

        private void PublishButtonRemoved(int index) {
            _eventBus.PublishButtonRemoved(CardGroupId, ButtonMode.Zone, index);
        }

    }
}
