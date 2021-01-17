using ArkhamOverlay.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace ArkhamOverlay.Data {
    public class Game : IGame, INotifyPropertyChanged {
        public Game() {
            Players = new List<Player> { new Player(1), new Player(2), new Player(3), new Player(4) };
            EncounterSets = new List<EncounterSet>();
            ScenarioCards = new SelectableCards(SelectableType.Scenario);
            LocationCards = new SelectableCards(SelectableType.Location);
            EncounterDeckCards = new SelectableCards(SelectableType.Encounter);
        }

        public string Name { get; set; }

        public string Scenario { get; set; }

        public IList<EncounterSet> EncounterSets { get; set;  }

        public SelectableCards ScenarioCards { get; }

        public SelectableCards LocationCards { get; }

        public SelectableCards EncounterDeckCards { get; }


        public IList<Player> Players { get; }

        public event Action PlayersChanged;

        public event Action EncounterSetsChanged;
        public void OnEncounterSetsChanged() {
            EncounterSetsChanged?.Invoke();
            OnPropertyChanged(nameof(EncounterSets));
            OnPropertyChanged(nameof(EncounterCardOptionsVisibility));
        }

        public Visibility EncounterCardOptionsVisibility { 
            get {
                return EncounterSets.Any() ? Visibility.Visible : Visibility.Collapsed;
            } 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler == null) {
                return;
            }
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void ClearAllCardsLists() {
            foreach (var selectableCards in AllSelectableCards) {
                selectableCards.ClearCards();
            }
        }

        public void OnPlayersChanged() {
            PlayersChanged?.Invoke();
            OnPropertyChanged(nameof(Players));
        }

        internal bool IsEncounterSetSelected(string code) {
            return EncounterSets.Any(x => x.Code == code);
        }

        public IList<SelectableCards> AllSelectableCards {
            get {
                var allDecks = new List<SelectableCards> {
                    ScenarioCards,
                    LocationCards,
                    EncounterDeckCards
                };
                
                foreach (var player in Players) {
                    allDecks.Add(player.SelectableCards);
                }
                return allDecks;
            }
        }

        internal void ClearAllCards() {
            foreach (var selectableCards in AllSelectableCards) {
                selectableCards.HideAllCards();
            }
        }

        internal void ClearAllCardSets() {
            foreach (var selectableCards in AllSelectableCards) {
                if (selectableCards.CardSet.IsDisplayedOnOverlay) {
                    selectableCards.CardSet.ToggleVisibility();
                }
            }
        }
    }
}
