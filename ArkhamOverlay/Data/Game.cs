using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Services;
using PageController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ArkhamOverlay.Data {
    public class Game : ViewModel, IGame {
        public Game() {
            Players = new List<Player> { new Player(CardGroupId.Player1), new Player(CardGroupId.Player2), new Player(CardGroupId.Player3), new Player(CardGroupId.Player4) };
            EncounterSets = new List<EncounterSet>();
            LocalPacks = new List<string>();
            ScenarioCards = new CardGroup(CardGroupId.Scenario);
            ScenarioCards.AddCardZone(new CardZone("Act/Agenda Bar", CardZoneLocation.Top));
            LocationCards = new CardGroup(CardGroupId.Locations);
            EncounterDeckCards = new CardGroup(CardGroupId.EncounterDeck);
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

        public IList<string> LocalPacks { get; set; }

        public CardGroup ScenarioCards { get; }

        public CardGroup LocationCards { get; }

        public CardGroup EncounterDeckCards { get; }

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
            foreach (var selectableCards in AllCardGroups) {
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

        public IList<CardGroup> AllCardGroups {
            get {
                var allCardGroups = new List<CardGroup> {
                    ScenarioCards,
                    LocationCards,
                    EncounterDeckCards
                };
                
                foreach (var player in Players) {
                    allCardGroups.Add(player.CardGroup);
                }
                return allCardGroups;
            }
        }

    }
}
