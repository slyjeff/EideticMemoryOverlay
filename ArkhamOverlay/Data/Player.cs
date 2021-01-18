using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ArkhamOverlay.Data {
    public class Player : INotifyPropertyChanged {
        public Player(int id) {
            ID = id;
            SelectableCards = new SelectableCards(SelectableType.Player);
        }

        public int ID { get; }

        public string DeckId { get; set; }

        [JsonIgnore]
        public SelectableCards SelectableCards { get; }

        [JsonIgnore]
        public BitmapImage InvestigatorImage { get; set; }

        [JsonIgnore]
        public IDictionary<string, int> Slots { get; set; }

        [JsonIgnore]
        public Visibility LoadedVisiblity { get { return string.IsNullOrEmpty(SelectableCards.Name) ? Visibility.Hidden : Visibility.Visible; } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler == null) {
                return;
            }
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnPlayerChanged() {
            OnPropertyChanged(nameof(LoadedVisiblity));
            OnPropertyChanged(nameof(InvestigatorImage));
        }
    }
}
