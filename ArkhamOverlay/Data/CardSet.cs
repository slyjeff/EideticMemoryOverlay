using ArkhamOverlay.CardButtons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ArkhamOverlay.Data {
    public class CardSet {
        private readonly SelectableCards _selectableCards;
        public CardSet(SelectableCards selectableCards) {
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
                Buttons[Buttons.IndexOf(cardSetButtonToReplace)] = new CardButton(this, _selectableCards, card);
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
            }
        }

        public void RemoveCard(CardButton cardInSetButton) {
            Buttons.Remove(cardInSetButton);
        }
    }
}
