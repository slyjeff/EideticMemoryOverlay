using Emo.Common.Services;
using Emo.Common.Utils;
using Emo.Events;
using Emo.Services;
using PageController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Emo.Data {
    public class Configuration : ViewModel, IConfiguration {
        private readonly IEventBus _eventBus;

        public Configuration(IEventBus eventBus) {
            _eventBus = eventBus;
            OverlayWidth = 1228;
            OverlayHeight = 720;
            CardHeight = 300;
            Packs = new List<Pack>();
        }

        private string _lastSavedFileName;
        public string LastSavedFileName {
            get => _lastSavedFileName;
            set {
                _lastSavedFileName = value;
                OnConfigurationChange();
            }
        }

        private bool _trackHealthAndSanity;
        public bool TrackHealthAndSanity {
            get => _trackHealthAndSanity;
            set {
                _trackHealthAndSanity = value;
                NotifyPropertyChanged(nameof(TrackHealthAndSanity));
                OnTrackPlayerStatsChanged();
                OnConfigurationChange();
            }
        }

        private bool _trackResources;
        public bool TrackResources {
            get => _trackResources;
            set {
                _trackResources = value;
                NotifyPropertyChanged(nameof(TrackResources));
                OnTrackPlayerStatsChanged();
                OnConfigurationChange();
            }
        }

        private bool _trackClues;
        public bool TrackClues {
            get => _trackClues;
            set {
                _trackClues = value;
                NotifyPropertyChanged(nameof(TrackClues));
                OnTrackPlayerStatsChanged();
                OnConfigurationChange();
            }
        }

        private void OnTrackPlayerStatsChanged() {
            _eventBus.PublishStatTrackingVisibilityChangedEvent(TrackHealthAndSanity || TrackResources || TrackClues);
        }


        private bool _seperateStatSnapshots;
        public bool SeperateStatSnapshots {
            get => _seperateStatSnapshots;
            set {
                _seperateStatSnapshots = value;
                OnConfigurationChange();
            }
        }

        private Color _overlayColor;
        public Color OverlayColor {
            get => _overlayColor;
            set {
                _overlayColor = value;
                NotifyPropertyChanged(nameof(OverlayColor));
                OnConfigurationChange();
            }
        }

        private int _overlayHeight;
        public int OverlayHeight {
            get => _overlayHeight;
            set {
                _overlayHeight = value;
                NotifyPropertyChanged(nameof(OverlayHeight));
                OnConfigurationChange();
            }
        }

        private int _overlayWidth;
        public int OverlayWidth {
            get => _overlayWidth;
            set {
                _overlayWidth = value;
                NotifyPropertyChanged(nameof(OverlayWidth));
                OnConfigurationChange();
            }
        }

        private int _cardHeight;
        public int CardHeight {
            get => _cardHeight;
            set {
                _cardHeight = value;

                NotifyPropertyChanged(nameof(CardHeight));
                OnConfigurationChange();
            }
        }

        private int _topCardZoneHeight;
        public int TopCardZoneHeight {
            get => _topCardZoneHeight;
            set {
                _topCardZoneHeight = value;

                NotifyPropertyChanged(nameof(TopCardZoneHeight));
                OnConfigurationChange();
            }
        }

        private int _bottomCardZoneHeight;
        public int BottomCardZoneHeight {
            get => _bottomCardZoneHeight;
            set {
                _bottomCardZoneHeight = value;

                NotifyPropertyChanged(nameof(BottomCardZoneHeight));
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

        private Point _scenarioCardsPosition = new Point(0, 0);
        public Point ScenarioCardsPosition {
            get => _scenarioCardsPosition;
            set {
                _scenarioCardsPosition = value;
                OnConfigurationChange();
            }
        }

        private Point _locationsPosition = new Point(0, 0);
        public Point LocationsPosition {
            get => _locationsPosition;
            set {
                _locationsPosition = value;
                OnConfigurationChange();
            }
        }

        private Point _encounterCardsPosition = new Point(0, 0);
        public Point EncounterCardsPosition {
            get => _encounterCardsPosition;
            set {
                _encounterCardsPosition = value;
                OnConfigurationChange();
            }
        }

        private Point _player1Position = new Point(0, 0);
        public Point Player1Position {
            get => _player1Position;
            set {
                _player1Position = value;
                OnConfigurationChange();
            }
        }

        private Point _player2Position = new Point(0, 0);
        public Point Player2Position {
            get => _player2Position;
            set {
                _player2Position = value;
                OnConfigurationChange();
            }
        }

        private Point _player3Position = new Point(0, 0);
        public Point Player3Position {
            get => _player3Position;
            set {
                _player3Position = value;
                OnConfigurationChange();
            }
        }

        private Point _player4Position = new Point(0, 0);
        public Point Player4Position {
            get => _player4Position;
            set {
                _player4Position = value;
                OnConfigurationChange();
            }
        }

        private Point _overlayPosition = new Point(0, 0);
        public Point OverlayPosition {
            get => _overlayPosition;
            set {
                _overlayPosition = value;
                OnConfigurationChange();
            }
        }

        private bool _useAutoSnapshot = false;
        public bool UseAutoSnapshot {
            get => _useAutoSnapshot;
            set {
                _useAutoSnapshot = value;
                NotifyPropertyChanged(nameof(UseAutoSnapshot));
                OnConfigurationChange();
            }
        }

        private string _autoSnapshotFilePath = "overlay.png";
        public string AutoSnapshotFilePath {
            get => _autoSnapshotFilePath;
            set {
                _autoSnapshotFilePath = value;
                NotifyPropertyChanged(nameof(AutoSnapshotFilePath));
                OnConfigurationChange();
            }
        }

        private string _localImagesDirectory;
        public string LocalImagesDirectory {
            get => _localImagesDirectory;
            set {
                _localImagesDirectory = value;
                NotifyPropertyChanged(nameof(LocalImagesDirectory));
                OnConfigurationChange();
            }
        }

        public event Action ConfigurationChanged;
        public void OnConfigurationChange() {
            ConfigurationChanged?.Invoke();
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
