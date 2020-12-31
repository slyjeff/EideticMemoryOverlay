using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ArkhamOverlay.Data {
    public class Player : INotifyPropertyChanged {
        public Player() {
            SelectableCards = new SelectableCards(SelectableType.Player);
        }

        public string DeckId { get; set; }

        [JsonIgnore]
        public SelectableCards SelectableCards { get; set; }

        [JsonIgnore]
        public BitmapImage InvestigatorImage { get; set; }

        [JsonIgnore]
        public IEnumerable<string> CardIds { get; set; }

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
