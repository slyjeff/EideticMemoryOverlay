using ArkhamOverlay.CardButtons;
using PageController;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            CardSet = new ObservableCollection<Card>();
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
        public ObservableCollection<Card> CardSet { get; set; }
        
        private bool _showCardSet;
        public bool ShowCardSet { 
            get => _showCardSet;
            set {
                _showCardSet = value;
                NotifyPropertyChanged(nameof(ShowCardSet));
            }
        }

        public bool Loading { get; internal set; }

        public event Action<Card, Card> CardVisibilityToggled;

        public void ToggleCardVisibility(Card card) {
            if (!card.IsVisible) {
                CardVisibilityToggled?.Invoke(card, null);
                return;
            }

            if (card.FlipSideCard != null) {
                card.FlipSideCard.Hide();
            }
            CardVisibilityToggled?.Invoke(card, card.FlipSideCard);
        }

        internal void ToggleCardSetVisibility() {
            if (_showSetButton == null) {
                return;
            }

            _showSetButton.LeftClick();
        }

        internal void ClearSelections() {
            foreach (var cardButtons in CardButtons) {
                if (!(cardButtons is Card card)) {
                    continue;
                }

                if (card.IsVisible) {
                    card.Hide();
                    CardVisibilityToggled?.Invoke(card, null);
                }
            }
        }
        
        public void AddCardToSet(Card card) {
            if (CardSet.Contains(card.FlipSideCard)) {
                CardSet[CardSet.IndexOf(card.FlipSideCard)] = card;
            } else {
                //don't add more than one copy unless it's a player card
                if (!card.IsPlayerCard && CardSet.Any(x => x == card)) {
                    return;
                }

                //if there's an act and this is an agenda, always add it to the left
                var index = CardSet.Count();
                if (card.Type == CardType.Agenda && CardSet.Any(x => x.Type == CardType.Act)) {
                    index = CardSet.IndexOf(CardSet.First(x => x.Type == CardType.Act));
                }

                CardSet.Insert(index, card);
            }

            if (_showSetButton.IsVisible) {
                OnSetUpdated();
            }

            UpdateShowSetButtonName();
            ShowCardSet = CardSet.Count > 0;
        }

        public void RemoveCardFromSet(Card card) {
            CardSet.RemoveAt(CardSet.IndexOf(card));

            OnSetUpdated();

            UpdateShowSetButtonName();
            ShowCardSet = CardSet.Count > 0;
        }


        private void UpdateShowSetButtonName() {
            if (_showSetButton == null) {
                return;
            }

            var buttonName = (Type == SelectableType.Scenario)
                ? "Act/Agenda Bar"
                : "Hand";

            if (CardSet.Any()) {
                _showSetButton.Name = "Show " + buttonName + " (" + CardSet.Count + ")";
            } else {
                _showSetButton.Name = "Right Click cards to add them to " + buttonName;
            }
        }

        internal void LoadCards(IEnumerable<Card> cards) {
            foreach (var card in cards) {
                card.SelectableCards = this;
            }

            var clearButton = new ClearButton { SelectableCards = this };

            var playerButtons = new List<ICardButton> { clearButton };

            if (Type == SelectableType.Scenario || Type == SelectableType.Player) {
                _showSetButton = new ShowSetButton { SelectableCards = this };
                playerButtons.Add(_showSetButton);
                UpdateShowSetButtonName();
            }

            playerButtons.AddRange(cards);
            CardButtons = playerButtons;
            NotifyPropertyChanged(nameof(CardButtons));
        }

        internal void ClearCards() {
            ClearSelections();
            CardButtons.Clear();
            NotifyPropertyChanged(nameof(CardButtons));
        }

        public event Action<SelectableCards, bool> SetUpdated;

        public void OnSetUpdated() {
            SetUpdated?.Invoke(this, _showSetButton.IsVisible);
        }

        public void SetHidden() {
            if (_showSetButton == null) {
                return;
            }

            _showSetButton.SetHidden();
        }
    }
}
