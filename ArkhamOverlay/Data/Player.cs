using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.CardButtons;
using PageController;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Common.Events;
using ArkhamOverlay.Events;

namespace ArkhamOverlay.Data {
    public class Player : ViewModel, IHasImageButton {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        private bool _isStatTrackingVisible = false;

        public Player(CardGroup deck) {
            SelectableCards = new SelectableCards(deck);
            Health = new Stat(StatType.Health, deck);
            Sanity = new Stat(StatType.Sanity, deck);
            Resources = new Stat(StatType.Resources, deck);
            Clues = new Stat(StatType.Clues, deck);

            Faction = Faction.Other;

            Resources.Value = 5;

            _eventBus.SubscribeToStatTrackingVisibilityChangedEvent(e => {
                _isStatTrackingVisible = e.IsVisible;
                NotifyPropertyChanged(nameof(StatTrackingVisibility));
            });
        }

        public int ID { 
            get {
                switch (SelectableCards.CardGroup) {
                    case CardGroup.Player1:
                        return 1;
                    case CardGroup.Player2:
                        return 2;
                    case CardGroup.Player3:
                        return 3;
                    case CardGroup.Player4:
                        return 4;
                    default:
                        return 1;
                }
            }
        }

        public string DeckId { get; set; }

        public SelectableCards SelectableCards { get; }

        public string Name { get { return SelectableCards.Name; } }

        CardType IHasImageButton.ImageCardType { get { return CardType.Investigator; } }

        public string ImageSource { get; set; }

        private ImageSource _image;
        public ImageSource Image { 
            get => _image; 
            set {
                _image = value;
                NotifyPropertyChanged(nameof(Image));
            }
        }

        private ImageSource _buttonImage;
        public ImageSource ButtonImage {
            get => _buttonImage;
            set {
                _buttonImage = value;
                NotifyPropertyChanged(nameof(ButtonImage));
            }
        }

        private byte[] _buttonImageAsBytes;
        public byte[] ButtonImageAsBytes {
            get => _buttonImageAsBytes;
            set {
                _buttonImageAsBytes = value;
                NotifyPropertyChanged(nameof(ButtonImageAsBytes));
            }
        }

        public string InvestigatorCode { get; set; }

        public IDictionary<string, int> Slots { get; set; }

        public Visibility LoadedVisiblity { get { return string.IsNullOrEmpty(SelectableCards.Name) ? Visibility.Hidden : Visibility.Visible; } }

        public void OnPlayerChanged() {
            NotifyPropertyChanged(nameof(LoadedVisiblity));
            NotifyPropertyChanged(nameof(Name));
            NotifyPropertyChanged(nameof(PlayerNameBrush));
            NotifyPropertyChanged(nameof(StatTrackingVisibility));
        }

        public Stat Health { get; }
        public Stat Sanity { get; }
        public Stat Resources { get; }
        public Stat Clues { get; }
        
        public Visibility StatTrackingVisibility { get { return string.IsNullOrEmpty(SelectableCards.Name) || !_isStatTrackingVisible ? Visibility.Collapsed : Visibility.Visible; } }
        public Brush PlayerNameBrush {
            get {
                switch (Faction) {
                    case Faction.Guardian: return new SolidColorBrush(Colors.DodgerBlue);
                    case Faction.Seeker: return new SolidColorBrush(Colors.DarkGoldenrod);
                    case Faction.Mystic: return new SolidColorBrush(Colors.MediumPurple);
                    case Faction.Rogue: return new SolidColorBrush(Colors.MediumSpringGreen);
                    case Faction.Survivor: return new SolidColorBrush(Colors.Red);
                    default: return new SolidColorBrush(Colors.Black);
                }
            }
        }

        public Faction Faction { get; set; }
    }

    public class Stat : ViewModel {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        private readonly StatType _statType;
        private readonly CardGroup _deck;

        public Stat(StatType statType, CardGroup deck) {
            _statType = statType;
            _deck = deck;
            var fileName = AppDomain.CurrentDomain.BaseDirectory + "Images\\" + GetImageFileName(statType);
            Image = new BitmapImage(new Uri(fileName));
            Increase = new UpdateStateCommand(this, true);
            Decrease = new UpdateStateCommand(this, false);
        }

        private string GetImageFileName(StatType statType) {
            switch (statType) {
                case StatType.Health:
                    return "health.png";
                case StatType.Sanity:
                    return "sanity.png";
                case StatType.Resources:
                    return "resource.png";
                case StatType.Clues:
                    return "clue.png";
                default:
                    return "health.png";
            }
        }

        public ImageSource Image { get; }

        private int _value;
        public int Value { 
            get => _value;
            set {
                _value = value;
                ValueChanged();
            }
        }

        public ICommand Increase { get; }
        public ICommand Decrease { get; }

        private void ValueChanged() {
            NotifyPropertyChanged(nameof(Value));
            _eventBus.PublishStatUpdatedEvent(_deck, _statType, _value);
        }
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