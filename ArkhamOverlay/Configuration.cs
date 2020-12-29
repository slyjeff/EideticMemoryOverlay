using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace ArkhamOverlay {

    public delegate void OverlayConfigurationChange();

    public class Configuration : INotifyPropertyChanged {
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
                OnPropertyChanged("OverlayHeight");
                OverlayConfigurationChanged?.Invoke();
            }
        }

        public int OverlayWidth {
            get => _overlayWidth;
            set {
                _overlayWidth = value;
                OnPropertyChanged("OverlayWidth");
                OverlayConfigurationChanged?.Invoke();
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

                OnPropertyChanged("CardHeight");
                OnPropertyChanged("CardClipRect");
                OnPropertyChanged("CardRadius");
                OnPropertyChanged("CardWidth");
                OverlayConfigurationChanged?.Invoke();
            }
        }

        public IList<Pack> Packs { get; set; }

        [JsonIgnore]
        public double CardWidth {
            get => _cardHeight * .716;
        }
        
        [JsonIgnore]
        public double CardRadius { get; set; }

        [JsonIgnore]
        public Rect CardClipRect { get; set; }

        [JsonIgnore]
        public IList<EncounterSet> EncounterSets {
            get {
                return (from pack in Packs
                        orderby pack.CyclePosition, pack.Position
                        from encounterSet in pack.EncounterSets
                        select encounterSet).ToList();
            }
        }

        public event OverlayConfigurationChange OverlayConfigurationChanged;

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

        public string Code { get; set; }

        public string Name { get; set; }

        public int CyclePosition { get; set; }
        public int Position { get; set; }


        public IList<EncounterSet> EncounterSets { get; set; }
    }

    public class EncounterSet {
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
