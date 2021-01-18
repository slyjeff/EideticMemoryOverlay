using PageController;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArkhamOverlay.Data {
    public class Player : ViewModel {
        public Player(int id) {
            ID = id;
            SelectableCards = new SelectableCards(SelectableType.Player);
            Health = new Stat("health.png");
            Sanity = new Stat("sanity.png");
            Resources = new Stat("resource.png");
            Clues = new Stat("clue.png");
        }

        public int ID { get; }

        public string DeckId { get; set; }

        public SelectableCards SelectableCards { get; }

        public BitmapImage InvestigatorImage { get; set; }

        public IDictionary<string, int> Slots { get; set; }

        public Visibility LoadedVisiblity { get { return string.IsNullOrEmpty(SelectableCards.Name) ? Visibility.Hidden : Visibility.Visible; } }

        public void OnPlayerChanged() {
            NotifyPropertyChanged(nameof(LoadedVisiblity));
            NotifyPropertyChanged(nameof(InvestigatorImage));
        }

        public Stat Health { get; }
        public Stat Sanity { get; }
        public Stat Resources { get; }
        public Stat Clues { get; }
    }

    public class Stat : ViewModel {
        public Stat(string imageFile) {
            var fileName = AppDomain.CurrentDomain.BaseDirectory + "Images\\" + imageFile;
            Image = new BitmapImage(new Uri(fileName));
            Increase = new UpdateStateCommand(this, true);
            Decrease = new UpdateStateCommand(this, false);
        }

        public ImageSource Image { get; }

        private int _value;
        public int Value { 
            get => _value;
            set {
                _value = value;
                NotifyPropertyChanged(nameof(Value));
            }
        }

        public ICommand Increase { get; }
        public ICommand Decrease { get; }
    }

    public class UpdateStateCommand : ICommand {
        private Stat _stat;
        private bool _increase;
        public UpdateStateCommand(Stat stat, bool increase) {
            _stat = stat;
            _increase = increase;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            if (_increase) {
                _stat.Value++;
                return;
            }

            if (_stat.Value > 0) {
                _stat.Value--;
            } 
        }
    }
}
