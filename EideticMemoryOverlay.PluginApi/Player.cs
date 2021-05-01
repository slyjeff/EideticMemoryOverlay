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
using System.Collections.Generic;
using System.Linq;
using EideticMemoryOverlay.PluginApi.Buttons;

namespace EideticMemoryOverlay.PluginApi {
    public abstract class Player : INotifyPropertyChanged, IHasImageButton {
        public Player(ICardGroup cardGroup) {
            CardGroup = cardGroup;

            Stats = new List<Stat>();
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
        public string DeckId { get; set; }

        public ICardGroup CardGroup { get; }

        public string Name { get { return CardGroup.Name; } }

        public void Clear() {
            CardGroup.ClearCards();
            CardGroup.Name = string.Empty;
            Image = default;
            OnPlayerChanged();
        }

        string IHasImageButton.ImageId { get { return Name; } }

        public IList<Stat> Stats { get; }

        protected Stat CreateStat(StatType statType) {
            var stat = new Stat(statType, CardGroup.Id);
            Stats.Add(stat);
            return stat;
        }

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

        public virtual IList<DeckListItem> GetDeckList() {
            var cards = from cardButton in CardGroup.CardButtons.OfType<CardInfoButton>()
                        select cardButton.CardInfo;

            var deckList = new List<DeckListItem>();
            foreach (var card in cards) {
                deckList.Add(new DeckListItem(card));
            }

            return deckList;
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

            CardGroup.PublishCardGroupChanged();
        }

        public virtual Point GetCropStartingPoint() {
            return new Point(0, 0);
        }
    }

    public class Stat : INotifyPropertyChanged {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        private readonly CardGroupId _deck;

        public Stat(StatType statType, CardGroupId deck) {
            StatType = statType;
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

        public StatType StatType { get; }

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
            _eventBus.PublishStatUpdated(_deck, StatType, _value);
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