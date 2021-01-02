using System;
using System.ComponentModel;

namespace ArkhamOverlay.Data {
    public class AppData : INotifyPropertyChanged {
        public AppData() {
            Game = new Game();
            Configuration = new Configuration {
                OverlayWidth = 1228,
                OverlayHeight = 720,
                CardHeight = 300
            };
        }

        public Game Game { get; }
        public Configuration Configuration { get; }
        public bool ShuttingDown { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler == null) {
                return;
            }
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnGameChanged() {
            OnPropertyChanged(nameof(Game));
        }
    }
}
