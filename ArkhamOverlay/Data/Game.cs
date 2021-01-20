using ArkhamOverlay.Services;
using PageController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ArkhamOverlay.Data {
    public class Game : ViewModel, IGame {
        public Game(Configuration configuration) {
            Players = new List<Player> { new Player(configuration, 1), new Player(configuration, 2), new Player(configuration, 3), new Player(configuration, 4) };
            EncounterSets = new List<EncounterSet>();
            ScenarioCards = new SelectableCards(SelectableType.Scenario);
            LocationCards = new SelectableCards(SelectableType.Location);
            EncounterDeckCards = new SelectableCards(SelectableType.Encounter);
        }

        public string Name { get; set; }

        public string Scenario { get; set; }
        
        private string _snapshotDirectory;
        public string SnapshotDirectory {
            get => _snapshotDirectory;
            set {
                _snapshotDirectory = value;
                NotifyPropertyChanged(nameof(SnapshotDirectory));
            }
        }

        public IList<EncounterSet> EncounterSets { get; set;  }

        public SelectableCards ScenarioCards { get; }

        public SelectableCards LocationCards { get; }

        public SelectableCards EncounterDeckCards { get; }

        public IList<Player> Players { get; }

        public event Action PlayersChanged;

        public event Action EncounterSetsChanged;
        public void OnEncounterSetsChanged() {
            EncounterSetsChanged?.Invoke();
            NotifyPropertyChanged(nameof(EncounterSets));
            NotifyPropertyChanged(nameof(EncounterCardOptionsVisibility));
        }

        public Visibility EncounterCardOptionsVisibility { 
            get {
                return EncounterSets.Any() ? Visibility.Visible : Visibility.Collapsed;
            } 
        }

        internal void ClearAllCardsLists() {
            foreach (var selectableCards in AllSelectableCards) {
                selectableCards.ClearCards();
            }
        }

        public void OnPlayersChanged() {
            PlayersChanged?.Invoke();
            NotifyPropertyChanged(nameof(Players));
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
    }
}
