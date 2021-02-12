using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ArkhamOverlay.Data {
    public class CardZone {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        private readonly SelectableCards _selectableCards;
        public CardZone(SelectableCards selectableCards) {
            Buttons = new ObservableCollection<CardButton>();
            _selectableCards = selectableCards;
        }

        public event Action VisibilityToggled;
        internal void ToggleVisibility() {
            VisibilityToggled?.Invoke();
        }

        public event Action<bool> IsDisplayedOnOverlayChanged;

        private bool _isDisplayedOnOverlay = false;

        public bool IsDisplayedOnOverlay {
            get => _isDisplayedOnOverlay;
            set {
                _isDisplayedOnOverlay = value;
                IsDisplayedOnOverlayChanged?.Invoke(_isDisplayedOnOverlay);
            }
        }

        public ObservableCollection<CardButton> Buttons { get; set; }

        public IEnumerable<ICardInstance> CardInstances { get => Buttons; }

        public void AddCard(CardTemplate card) {
            var cardSetButtonToReplace = Buttons.FirstOrDefault(x => x.CardTemplate == card.FlipSideCard);
            if (cardSetButtonToReplace != null) {
                var index = Buttons.IndexOf(cardSetButtonToReplace);
                Buttons[index] = new CardButton(this, _selectableCards, card);
                PublishCardButtonInfoChanged(index);
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

                Buttons.Insert(index, new CardButton(this, _selectableCards, card));
                PublishCardButtonInfoChangedRange(index, Buttons.Count - 1);
            }
        }

        public void RemoveCard(CardButton cardButton) {
            var indexOfRemovedCard = Buttons.IndexOf(cardButton);
            Buttons.Remove(cardButton);
            PublishCardButtonInfoChangedRange(indexOfRemovedCard, Buttons.Count); //not an off by one error- we send an update for the card at the index one after the list so it can be removed
        }

        public void PublishCardButtonInfoChangedRange(int startIndex, int endIndex) {
            for (int index = startIndex; index <= endIndex; index++) {
                PublishCardButtonInfoChanged(index);
            }
        }

        private void PublishCardButtonInfoChanged(int index) {
            var button = index < Buttons.Count ? Buttons[index] : null;

            var cardGroup = _selectableCards.CardGroup;
            var cardName = button?.Text;
            var isToggled = button != null && button.IsToggled;
            var isImageAvailable = button?.CardTemplate.ButtonImageAsBytes != null;

            _eventBus.PublishButtonInfoChanged(cardGroup, 1, index, cardName, isToggled, isImageAvailable);
        }
    }
}
