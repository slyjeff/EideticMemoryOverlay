using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArkhamOverlay {
    public class Game : INotifyPropertyChanged {
        public Game() {
            Players = new List<Player>();
            EncounterSets = new List<EncounterSet>();
        }

        public string Name { get; set; }

        public string Scenario { get; set; }

        public IList<EncounterSet> EncounterSets { get; set; }

        public IList<Player> Players { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler == null) {
                return;
            }
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnPlayersChanged() {
            OnPropertyChanged(nameof(Players));
        }
        public void OnEncounterSetsChanged() {
            OnPropertyChanged(nameof(EncounterSets));
        }
    }

    public interface IPlayer {
        string Investigator { get; }

        string DeckId { get; }

        BitmapImage InvestigatorImage { get; }

        List<IPlayerButton> PlayerButtons { get; }

        IEnumerable<string> CardIds { get; }

        bool Loading { get; }

        Visibility LoadedVisiblity { get; }
    }

    public class Player : IPlayer, INotifyPropertyChanged {
        public Player() {
            PlayerButtons = new List<IPlayerButton>();
        }

        public string Investigator { get; set; }

        public string DeckId { get; set; }

        [JsonIgnore]
        public BitmapImage InvestigatorImage { get; set; }

        [JsonIgnore]
        public List<IPlayerButton> PlayerButtons { get; set; }

        [JsonIgnore]
        public IEnumerable<string> CardIds { get; set; }

        [JsonIgnore]
        public bool Loading { get; internal set; }

        [JsonIgnore]
        public Visibility LoadedVisiblity { get { return string.IsNullOrEmpty(Investigator) ? Visibility.Hidden : Visibility.Visible; } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler == null) {
                return;
            }
            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnPlayerChanged() {
            OnPropertyChanged(nameof(InvestigatorImage));
            OnPropertyChanged(nameof(LoadedVisiblity));
        }

        public void OnPlayerCardsChanged() {
            OnPropertyChanged(nameof(PlayerButtons));
        }
    }

    public interface IPlayerButton {
        Brush Background { get; }
    }

    public class Card : IPlayerButton {
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

    public class ClearButton : IPlayerButton {
        public Brush Background {
            get {
                return new SolidColorBrush(Colors.Black);
            }
        }
    }
}
