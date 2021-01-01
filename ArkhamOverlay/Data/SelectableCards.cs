using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ArkhamOverlay.Data {
    public interface ISelectableCards {
        SelectableType Type { get; }

        string Name { get; }

        List<ICardButton> CardButtons { get; }

        bool Loading { get; }
    }

    public class SelectableCards : ISelectableCards, INotifyPropertyChanged {
        private string _playerName = string.Empty;

        public SelectableCards(SelectableType type) {
            Type = type;
            CardButtons = new List<ICardButton>();
        }

        public SelectableType Type { get; }

        public string Name { 
            get {
                switch(Type) {
                    case SelectableType.Scenario:
                        return "Act/Agenda/Scenario Reference";
                    case SelectableType.Location:
                        return "Location";
                    case SelectableType.Encounter:
                        return "Encounter Deck";
                    case SelectableType.Player:
                        return _playerName;
                    default:
                        return "Unknown";
                }
            }
            set {
                if (Type == SelectableType.Player) {
                    _playerName = value;
                }
            }
        }

        public List<ICardButton> CardButtons { get; set; }

        public bool Loading { get; internal set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler == null) {
                return;
            }
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnCardButtonsChanged() {
            OnPropertyChanged(nameof(CardButtons));
        }

        public event Action<Card, Card> CardToggled;

        public void ToggleCard(Card card) {
            if (card.IsVisible) {
                CardToggled?.Invoke(card, null);
                return;
            }

            CardToggled?.Invoke(card, card.FlipSideCard);
            if (card.FlipSideCard != null) {
                card.FlipSideCard.Hide();
            }
        }

        internal void Load(IEnumerable<Card> cards) {
            foreach (var card in cards) {
                card.SelectableCards = this;
            }

            var clearButton = new ClearButton { SelectableCards = this };

            var playerButtons = new List<ICardButton> { clearButton };
            playerButtons.AddRange(cards);
            CardButtons = playerButtons;
            OnCardButtonsChanged();
        }

        internal void ClearSelections() {
            foreach (var cardButtons in CardButtons) {
                if (!(cardButtons is Card card)) {
                    continue;
                }

                if (card.IsVisible) {
                    card.Click();
                }
            }
        }
    }
}
