using Emo.Common.Enums;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emo.Common.Services;
using Emo.Common.Utils;
using Emo.Common.Events;
using System.ComponentModel;

namespace EideticMemoryOverlay.PluginApi {
    public class Player : INotifyPropertyChanged, IHasImageButton {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        private bool _isStatTrackingVisible = false;

        public Player(CardGroupId deck, IPlugIn plugIn) {
            CardGroup = new CardGroup(deck, plugIn);

            _eventBus.SubscribeToStatTrackingVisibilityChangedEvent(e => {
                _isStatTrackingVisible = e.IsVisible;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatTrackingVisibility)));
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string property) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public int ID {
            get {
                switch (CardGroup.Id) {
                    case CardGroupId.Player1:
                        return 1;
                    case CardGroupId.Player2:
                        return 2;
                    case CardGroupId.Player3:
                        return 3;
                    case CardGroupId.Player4:
                        return 4;
                    default:
                        return 1;
                }
            }
        }

        public CardGroup CardGroup { get; }

        public string Name { get { return CardGroup.Name; } }

        string IHasImageButton.ImageId { get { return Name; } }

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

                //the button image is set when the source image is loaded- but we need some other images for the overlay.
                LoadOverlayImages();
            }
        }

        protected virtual void LoadOverlayImages() {
        }

        private byte[] _buttonImageAsBytes;

        public byte[] ButtonImageAsBytes {
            get => _buttonImageAsBytes;
            set {
                _buttonImageAsBytes = value;
                CardGroup.ButtonImageAsBytes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonImageAsBytes)));
            }
        }

        public Visibility LoadedVisiblity { get { return string.IsNullOrEmpty(CardGroup.Name) ? Visibility.Hidden : Visibility.Visible; } }

        public virtual void OnPlayerChanged() {
            NotifyPropertyChanged(nameof(LoadedVisiblity));
            NotifyPropertyChanged(nameof(Name));
            NotifyPropertyChanged(nameof(StatTrackingVisibility));

            CardGroup.PublishCardGroupChanged();
        }

        public virtual Point GetCropStartingPoint() {
            return new Point(0, 0);
        }

        public Visibility StatTrackingVisibility { get { return string.IsNullOrEmpty(CardGroup.Name) || !_isStatTrackingVisible ? Visibility.Collapsed : Visibility.Visible; } }
    }

    public class Stat : INotifyPropertyChanged {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        private readonly StatType _statType;
        private readonly CardGroupId _deck;

        public Stat(StatType statType, CardGroupId deck) {
            _statType = statType;
            _deck = deck;
            var fileName = AppDomain.CurrentDomain.BaseDirectory + "Images\\" + GetImageFileName(statType);
            Image = new BitmapImage(new Uri(fileName));
            Increase = new UpdateStateCommand(this, true);
            Decrease = new UpdateStateCommand(this, false);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string property) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
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

        public int Max { get; set; }

        public string DisplayValue {
            get {
                if (Max > 0) {
                    return $"{Value}/{Max}";
                }
                return Value.ToString();
            }
        }

        public ICommand Increase { get; }
        public ICommand Decrease { get; }

        private void ValueChanged() {
            NotifyPropertyChanged(nameof(Value));
            NotifyPropertyChanged(nameof(DisplayValue));
            _eventBus.PublishStatUpdated(_deck, _statType, _value);
        }
    }

    public class UpdateStateCommand : ICommand {
        private readonly Stat _stat;
        private readonly bool _increase;
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