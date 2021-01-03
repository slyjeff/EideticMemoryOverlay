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

                CardClipRect = new Rect {
                    Height = _cardHeight,
                    Width = _cardHeight * 0.716
                };
                CardRadius = _cardHeight / 30;

                OnPropertyChanged(nameof(CardHeight));
                OnPropertyChanged(nameof(CardClipRect));
                OnPropertyChanged(nameof(CardRadius));
                OnPropertyChanged(nameof(CardWidth));
                OnConfigurationChange();
            }
        }

        public IList<Pack> Packs { get; set; }

        public double CardWidth {
            get => _cardHeight * .716;
        }
        
        public double CardRadius { get; set; }

        public Rect CardClipRect { get; set; }

        public IList<EncounterSet> EncounterSets {
            get {
                return (from pack in Packs
                        orderby pack.CyclePosition, pack.Position
                        from encounterSet in pack.EncounterSets
                        select encounterSet).ToList();
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
