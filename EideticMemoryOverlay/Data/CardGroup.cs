using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.Buttons;
using EideticMemoryOverlay.PluginApi.Interfaces;
using Emo.Common.Enums;
using Emo.Common.Services;
using Emo.Common.Utils;
using Emo.Events;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Emo.Data {
    /// <summary>
    /// A logical grouping of cards that contains a pool (cards avaiable for use by this card group) and (optionally) CardZone(s) that
    /// represent physical locations of instances of cards in the real world
    /// </summary>
    internal class CardGroup : ICardGroup, INotifyPropertyChanged {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        private string _playerName = string.Empty;
        private readonly IList<CardZone> _cardZones = new List<CardZone>();
        private readonly IPlugIn _plugIn;

        public CardGroup(CardGroupId id, IPlugIn plugIn) {
            Type = id.GetSelectableType();
            Id = id;
            _plugIn = plugIn;
            CardButtons = new List<IButton>();
            _cardZones = new List<CardZone>();

            _eventBus.SubscribeToCardInfoVisibilityChanged(CardInfoVisibilityChangedHandler);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string property) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public CardGroupType Type { get; }

        public CardGroupId Id { get; }

        public string Name {
            get {
                switch (Type) {
                    case CardGroupType.Scenario:
                        return "Act/Agenda/Scenario Reference";
                    case CardGroupType.Location:
                        return "Location";
                    case CardGroupType.Encounter:
                        return "Encounter Deck";
                    case CardGroupType.Player:
                        return _playerName;
                    default:
                        return "Unknown";
                }
            }
            set {
                if (Type == CardGroupType.Player) {
                    _playerName = value;
                }
            }
        }

        public string CardZoneName { get { return _cardZones.Any() ? _cardZones[0].Name : ""; } }


        private List<IButton> _cardButtons = new List<IButton>();
        public List<IButton> CardButtons { 
            get => _cardButtons; 
            set {
                _cardButtons = value;
                NotifyPropertyChanged(nameof(CardButtons));
            }
        }

        private byte[] _buttonImageAsBytes { get; set; }
        /// <summary>
        /// Image associated with this card group
        /// </summary>
        public byte[] ButtonImageAsBytes {
            get => _buttonImageAsBytes;
            set {
                _buttonImageAsBytes = value;
                PublishCardGroupChanged();
            }
        }

        /// <summary>
        /// Notify listeners that the card group has changed
        /// </summary>
        public void PublishCardGroupChanged() {
            var zoneNames = _cardZones.Select(x => x.Name).ToList();
            _eventBus.PublishCardGroupChanged(Id, Name, ButtonImageAsBytes != null, Name, zoneNames);
        }

        /// <summary>
        /// Add a Card Zone to this Card Group
        /// </summary>
        /// <param name="cardZone">Card Zone to add</param>
        public void AddCardZone(CardZone cardZone) {
            cardZone.CardGroupId = Id;
            cardZone.ZoneIndex = _cardZones.Count;
            _cardZones.Add(cardZone);
        }

        /// <summary>
        /// Retrieve a Card Zone by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Card Zone at the index- default(CardZone) if does not exist</returns>
        public CardZone GetCardZone(int index) {
            if (index >= _cardZones.Count) {
                return default;
            }

            return _cardZones[index];
        }

        public IList<CardZone> CardZones { get { return _cardZones; } }

        public bool Loading { get; set; }
        public IEnumerable<CardInfo> CardPool { get => CardButtons.OfType<CardInfoButton>().Select(x => x.CardInfo); }

        /// <summary>
        /// Get a list of all buttons in this card group 
        /// </summary>
        /// <returns>list of button info for every button in this card group</returns>
        /// <remark>used for sending update events</remark>
        public IList<ButtonInfo> GetButtonInfo() {
            var buttonInfoList = new List<ButtonInfo>();

            var poolButtonIndex = 0;
            foreach (var button in CardButtons) {
                buttonInfoList.Add(CreateButtonInfo(ButtonMode.Pool, zoneIndex: 0, poolButtonIndex++, button));
            }

            foreach (var zone in CardZones) {
                var zoneButtonIndex = 0;
                foreach (var button in zone.Buttons) {
                    buttonInfoList.Add(CreateButtonInfo(ButtonMode.Zone, zone.ZoneIndex, zoneButtonIndex++, button));
                }
            }

            return buttonInfoList;
        }

        /// <summary>
        /// Find a button
        /// </summary>
        /// <param name="context">Information to find the button</param>
        /// <returns>The button identified by the context- default if not found</returns>
        public IButton GetButton(IButtonContext context) {
            if (context.CardGroupId != Id) {
                return default(Button);
            }

            return GetButton(context.ButtonMode, context.ZoneIndex, context.Index);
        }

        /// <summary>
        /// Find a button
        /// </summary>
        /// <param name="buttonMode">Look in the pool or in card zones</param>
        /// <param name="zoneIndex">index of zone the button- does not apply for pool</param>
        /// <param name="index">index of the button</param>
        /// <returns>The button identified by the parameters- default if not found</returns>
        public IButton GetButton(ButtonMode buttonMode, int zoneIndex, int index) {
            if (buttonMode == ButtonMode.Pool) {
                return index < CardButtons.Count ? CardButtons[index] : default(Button);
            }

            var cardZone = GetCardZone(zoneIndex);
            if (cardZone == default(CardZone)) {
                return default(Button);
            }

            return index < cardZone.Buttons.Count ? cardZone.Buttons[index] : default(Button);
        }

        /// <summary>
        /// Remove all CardInfos from the pool and all card zones
        /// </summary>
        public void ClearCards() {
            CardButtons.Clear();
            foreach (var cardZone in _cardZones) {
                cardZone.ClearButtons();
            }

            NotifyPropertyChanged(nameof(CardButtons));
        }

        /// <summary>
        /// Replace the cards in this group with a new set of cards
        /// </summary>
        /// <param name="cards">new cards to use</param>
        public void LoadCards(IEnumerable<CardInfo> cards) {
            var clearButton = new ClearButton();

            var playerButtons = new List<IButton> { clearButton };

            var cardInfoButtons = SortCards(cards).Select(x => _plugIn.CreateCardInfoButton(x, Id)).ToList();
            playerButtons.AddRange(cardInfoButtons);
            CardButtons = playerButtons;

            RegisterButtonImageLoadCallbacks();
            _eventBus.PublishCardGroupButtonsChanged(Id, GetButtonInfo());
        }

        /// <summary>
        /// Create an object with information required for update events
        /// </summary>
        /// <param name="buttonMode">Whether this is a pool or zone button</param>
        /// <param name="zoneIndex">Index of the zone, if applicable</param>
        /// <param name="index">Index of the button</param>
        /// <param name="button">Get info from this button</param>
        /// <returns>Object with information required for update events</returns>
        private ButtonInfo CreateButtonInfo(ButtonMode buttonMode, int zoneIndex, int index, IButton button) {
            var cardImageButton = button as CardImageButton;

            return new ButtonInfo {
                CardGroupId = Id,
                ButtonMode = buttonMode,
                ZoneIndex = zoneIndex,
                Index = index,
                Name = button.Text,
                Code = cardImageButton == null ? string.Empty : cardImageButton.CardInfo.Code,
                IsToggled = button.IsToggled,
                ImageAvailable = cardImageButton != null,
                ButtonOptions = button.Options
            };
        }

        /// <summary>
        /// For any buttons that don't have images loaded, register events that will publish a change when the images finally load
        /// </summary>
        private void RegisterButtonImageLoadCallbacks() {
            foreach (var button in CardButtons.OfType<CardInfoButton>()) {
                var cardInfo = button.CardInfo;
                if (cardInfo.ButtonImageAsBytes != null) {
                    return;
                }

                button.CardInfo.ButtonImageLoaded += () => {
                    _eventBus.PublishButtonInfoChanged(Id, ButtonMode.Pool, zoneIndex: 0, CardButtons.IndexOf(button), button.Text, button.CardInfo.ImageId, button.IsToggled, imageAvailable: true, ChangeAction.Update, button.Options);
                };
            }
        }

        /// <summary>
        /// Look through all card zones to find this button and remove it
        /// </summary>
        /// <param name="button">The button to remove</param>
        public void RemoveCard(CardButton button) {
            foreach (var zone in _cardZones) {
                zone.RemoveButton(button);
            }
        }

        private IEnumerable<CardInfo> SortCards(IEnumerable<CardInfo> cards) {
            var firstCard = cards.FirstOrDefault();
            if (firstCard == null) {
                return cards;
            }

            //todo: change sorting in card group to be enumartion based rather than looking at the first card
            if (firstCard.SortBy == CardInfoSort.Natural) {
                return cards;
            }

            var sortedCards = cards.OrderBy(x => x.Name.Replace("\"", "")).ToList();
            return sortedCards;
        }

        /// <summary>
        /// When card info visibility has changed, look through all of our buttons to see if we need to show that they are visible
        /// </summary>
        /// <param name="e">CardInfoVisibilityChanged</param>
        private void CardInfoVisibilityChangedHandler(CardInfoVisibilityChanged e) {
            var cardImageButtons = CardButtons.OfType<CardImageButton>();
            foreach (var cardZone in _cardZones) {
                cardImageButtons = cardImageButtons.Union(cardZone.CardButtons.OfType<CardImageButton>());
            }

            foreach (var button in cardImageButtons) {
                if (e.Code == button.CardInfo.ImageId) {
                    button.IsToggled = e.IsVisible;
                }
            }
        }
    }

}
