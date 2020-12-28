using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArkhamOverlay {
    public class Game : INotifyPropertyChanged {
        public Game() {
            Players = new List<Player>();
        }

        public string Name { get; set; }

        public List<Player> Players { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler == null) {
                return;
            }
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnPlayersChanged() {
            OnPropertyChanged("Players");
        }
    }

    public class Player : INotifyPropertyChanged {
        public Player() {
            Cards = new List<Card>();
        }

        public string DeckId { get; set; }

        [JsonIgnore]
        public BitmapImage InvestigatorImage { get; set; }

        [JsonIgnore]
        public IList<Card> Cards { get; set; }

        [JsonIgnore]
        public IEnumerable<string> CardIds { get; set; }

        [JsonIgnore]
        public bool Loading { get; internal set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler == null) {
                return;
            }
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnPlayerChanged() {
            OnPropertyChanged("InvestigatorImage");
        }

        public void OnPlayerCardsChanged() {
            OnPropertyChanged("Cards");
        }
    }

    public class Card {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Faction { get; set; }
        public string ImageSource { get; set; }
        public Brush Background {
            get {
                if (Faction == "Guardian") {
                    return new SolidColorBrush(Colors.DarkBlue);
                }

                if (Faction == "Seeker") {
                    return new SolidColorBrush(Colors.DarkGoldenrod);
                }

                if (Faction == "Rogue") {
                    return new SolidColorBrush(Colors.DarkGreen);
                }

                if (Faction == "Survivor") {
                    return new SolidColorBrush(Colors.DarkRed);
                }

                if (Faction == "Mystic") {
                    return new SolidColorBrush(Colors.Indigo);
                }

                return new SolidColorBrush(Colors.DarkGray);
            }
        }
    }
}
