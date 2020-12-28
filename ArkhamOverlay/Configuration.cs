using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Windows;

namespace ArkhamOverlay {

    public delegate void OverlayConfigurationChange();

    public class Configuration : INotifyPropertyChanged {
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
                OverlayConfigurationChanged?.Invoke();
            } 
        }
        
        [JsonIgnore]
        public double CardRadius { get; set; }

        [JsonIgnore]
        public Rect CardClipRect { get; set; }

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
}
