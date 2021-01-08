using ArkhamOverlay.CardButtons;
using PageController;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ArkhamOverlay.Data {
    public interface ISelectableCards {
        SelectableType Type { get; }

        string Name { get; }

        List<ICardButton> CardButtons { get; }
        
        bool Loading { get; }
    }

    public class SelectableCards : ViewModel, ISelectableCards, INotifyPropertyChanged {
        private string _playerName = string.Empty;

        public SelectableCards(SelectableType type) {
            Type = type;
            CardButtons = new List<ICardButton>();
            CardButtonSet = new ObservableCollection<ICardButton>();
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

        public string CardButtonSetName {
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
        public ObservableCollection<ICardButton> CardButtonSet { get; set; }
        
        private bool _showCardButtonSet;
        public bool ShowCardButtonSet { 
            get => _showCardButtonSet;
            set {
                _showCardButtonSet = value;
                NotifyPropertyChanged(nameof(ShowCardButtonSet));
            }
        }

        public bool Loading { get; internal set; }

        public event Action<Card, Card> CardToggled;

        public void ToggleCardVisibility(Card card) {
            if (!card.IsVisible) {
                CardToggled?.Invoke(card, null);
                return;
            }

            if (card.FlipSideCard != null) {
                card.FlipSideCard.Hide();
            }
            CardToggled?.Invoke(card, card.FlipSideCard);
        }

        internal void ClearSelections() {
            foreach (var cardButtons in CardButtons) {
                if (!(cardButtons is Card card)) {
                    continue;
                }

                if (card.IsVisible) {
                    card.Hide();
                    CardToggled?.Invoke(card, null);
                }
            }
        }
        
        public void ToggleCardInSet(Card card) {
            if (CardButtonSet.Contains(card)) {
                CardButtonSet.Remove(card);
            } else if (CardButtonSet.Contains(card.FlipSideCard)) {
                CardButtonSet[CardButtonSet.IndexOf(card.FlipSideCard)] = card;
            } else {
                CardButtonSet.Add(card);
            }

            ShowCardButtonSet = CardButtonSet.Count > 0;
        }

        internal void LoadCards(IEnumerable<Card> cards) {
            foreach (var card in cards) {
                card.SelectableCards = this;
            }

            var clearButton = new ClearButton { SelectableCards = this };

            var playerButtons = new List<ICardButton> { clearButton };
            playerButtons.AddRange(cards);
            CardButtons = playerButtons;
            NotifyPropertyChanged(nameof(CardButtons));
        }

        internal void ClearCards() {
            ClearSelections();
            CardButtons.Clear();
            NotifyPropertyChanged(nameof(CardButtons));
        }
    }
}
