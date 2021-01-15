using ArkhamOverlay.CardButtons;
using PageController;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ArkhamOverlay.Data {
    public interface ISelectableCards {
        SelectableType Type { get; }

        string Name { get; }

        List<ICardButton> CardButtons { get; }
        
        bool Loading { get; }
    }

    public class SelectableCards : ViewModel, ISelectableCards, INotifyPropertyChanged {
        private string _playerName = string.Empty;
        private ShowSetButton _showSetButton = null;

        public SelectableCards(SelectableType type) {
            Type = type;
            CardButtons = new List<ICardButton>();
            CardSet = new CardSet(this);
            CardSet.Buttons.CollectionChanged += (s, e) => CardSetUpdated();
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

        public string CardSetName {
            get {
                switch (Type) {
                    case SelectableType.Scenario:
                        return "Act/Agena Bar:";
                    case SelectableType.Player:
                        return "In Hand:";
                    default:
                        return "";
                }
            }
        }

        public List<ICardButton> CardButtons { get; set; }
        public CardSet CardSet { get; }
        
        private bool _showCardSetButtons;
        public bool ShowCardSetButtons { 
            get => _showCardSetButtons;
            set {
                _showCardSetButtons = value;
                NotifyPropertyChanged(nameof(ShowCardSetButtons));
            }
        }

        public bool Loading { get; internal set; }


        public event Action<Card> CardVisibilityToggled;
        public void ToggleCardVisibility(Card card) {
            CardVisibilityToggled?.Invoke(card);
        }

        public event Action<ICardButton> ButtonChanged;
        public void OnButtonChanged(ICardButton button) {
            ButtonChanged?.Invoke(button);
        }


        internal void ToggleCardSetVisibility() {
            if (_showSetButton == null) {
                return;
            }

            _showSetButton.LeftClick();
        }


        internal void HideAllCards() {
            foreach (var showCardButton in CardButtons.OfType<ShowCardButton>()) {
                if (showCardButton.Card.IsDisplayedOnOverlay) {
                    CardVisibilityToggled?.Invoke(showCardButton.Card);
                }
            }
        }

        private void CardSetUpdated() {
            UpdateShowSetButtonName();
            ShowCardSetButtons = CardSet.Buttons.Count > 0;
        }

        private void UpdateShowSetButtonName() {
            if (_showSetButton == null) {
                return;
            }

            var buttonName = (Type == SelectableType.Scenario)
                ? "Act/Agenda Bar"
                : "Hand";

            if (CardSet.Buttons.Any()) {
                _showSetButton.Text = "Show " + buttonName + " (" + CardSet.Buttons.Count + ")";
            } else {
                _showSetButton.Text = "Right Click to add cards to " + buttonName;
            }
        }

        internal void LoadCards(IEnumerable<Card> cards) {
            var clearButton = new ClearButton(this);

            var playerButtons = new List<ICardButton> { clearButton };

            if (Type == SelectableType.Scenario || Type == SelectableType.Player) {
                _showSetButton = new ShowSetButton(this);
                playerButtons.Add(_showSetButton);
                UpdateShowSetButtonName();
            }

            playerButtons.AddRange(from card in cards select new ShowCardButton(this, card));
            CardButtons = playerButtons;
            NotifyPropertyChanged(nameof(CardButtons));
        }

        internal void ClearCards() {
            HideAllCards();
            CardButtons.Clear();
            NotifyPropertyChanged(nameof(CardButtons));
        }
    }
}
