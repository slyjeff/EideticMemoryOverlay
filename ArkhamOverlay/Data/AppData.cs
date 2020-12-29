using System.ComponentModel;

namespace ArkhamOverlay.Data {
    public class AppData : INotifyPropertyChanged {
        public Game Game { get; set; }

        public Configuration Configuration { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler == null) {
                return;
            }
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnGameChanged() {
            OnPropertyChanged("Game");
        }
    }
}
