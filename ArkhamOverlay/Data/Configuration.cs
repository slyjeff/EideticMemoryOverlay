using ArkhamOverlay.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace ArkhamOverlay.Data {
    public class Configuration : IConfiguration, INotifyPropertyChanged {
        public Configuration() {
            Packs = new List<Pack>();
        }

        private int _overlayHeight;
        private int _overlayWidth;
        private int _cardHeight;

        public int OverlayHeight {
            get => _overlayHeight;
            set {
                _overlayHeight = value;
                OnPropertyChanged(nameof(OverlayHeight));
                OnConfigurationChange();
            }
        }

        public int OverlayWidth {
            get => _overlayWidth;
            set {
                _overlayWidth = value;
                OnPropertyChanged(nameof(OverlayWidth));
                OnConfigurationChange();
            }
        }

        public int CardHeight {
            get => _cardHeight;
            set {
                _cardHeight = value;

                OnPropertyChanged(nameof(CardHeight));
                OnConfigurationChange();
            }
        }

        public int _actAgendaCardHeight;
        public int ActAgendaCardHeight {
            get => _actAgendaCardHeight;
            set {
                _actAgendaCardHeight = value;

                OnPropertyChanged(nameof(ActAgendaCardHeight));
                OnConfigurationChange();
            }
        }

        public int _handCardHeight;
        public int HandCardHeight {
            get => _handCardHeight;
            set {
                _handCardHeight = value;

                OnPropertyChanged(nameof(HandCardHeight));
                OnConfigurationChange();
            }
        }

        public IList<Pack> Packs { get; set; }

        public IList<EncounterSet> EncounterSets {
            get {
                return (from pack in Packs
                        orderby pack.CyclePosition, pack.Position
                        from encounterSet in pack.EncounterSets
                        select encounterSet).ToList();
            }
        }

        public Point _scenarioCardsPosition = new Point(0, 0);
        public Point ScenarioCardsPosition {
            get => _scenarioCardsPosition;
            set {
                _scenarioCardsPosition = value;
                OnConfigurationChange();
            }
        }

        public Point _locationsPosition = new Point(0, 0);
        public Point LocationsPosition {
            get => _locationsPosition;
            set {
                _locationsPosition = value;
                OnConfigurationChange();
            }
        }

        public Point _encounterCardsPosition = new Point(0, 0);
        public Point EncounterCardsPosition {
            get => _encounterCardsPosition;
            set {
                _encounterCardsPosition = value;
                OnConfigurationChange();
            }
        }

        public Point _player1Position = new Point(0, 0);
        public Point Player1Position {
            get => _player1Position;
            set {
                _player1Position = value;
                OnConfigurationChange();
            }
        }

        public Point _player2Position = new Point(0, 0);
        public Point Player2Position {
            get => _player2Position;
            set {
                _player2Position = value;
                OnConfigurationChange();
            }
        }

        public Point _player3Position = new Point(0, 0);
        public Point Player3Position {
            get => _player3Position;
            set {
                _player3Position = value;
                OnConfigurationChange();
            }
        }

        public Point _player4Position = new Point(0, 0);
        public Point Player4Position {
            get => _player4Position;
            set {
                _player4Position = value;
                OnConfigurationChange();
            }
        }

        public Point _overlayPosition = new Point(0, 0);
        public Point OverlayPosition {
            get => _overlayPosition;
            set {
                _overlayPosition = value;
                OnConfigurationChange();
            }
        }

        public event Action ConfigurationChanged;
        public void OnConfigurationChange() {
            ConfigurationChanged?.Invoke();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler == null) {
                return;
            }
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Pack {
        public Pack() {
            EncounterSets = new List<EncounterSet>();
        }

        public Pack(Pack pack) {
            Code = pack.Code;
            Name = pack.Name;
            CyclePosition = pack.CyclePosition;
            Position = pack.Position;

            EncounterSets = new List<EncounterSet>();
            foreach (var encounterSet in pack.EncounterSets) {
                EncounterSets.Add(new EncounterSet(encounterSet));
            }
        }

        public string Code { get; set; }

        public string Name { get; set; }

        public int CyclePosition { get; set; }

        public int Position { get; set; }

        public IList<EncounterSet> EncounterSets { get; set; }
    }

    public class EncounterSet {
        public EncounterSet() {
        }

        public EncounterSet(EncounterSet encounterSet) {
            Name = encounterSet.Name;
            Code = encounterSet.Code;
        }

        public string Name { get; set; }
        public string Code { get; set; }
    }
}
