using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Events;
using PageController;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ArkhamOverlay.Data {
    public interface ICardGroup {
        CardGroupId Id { get; }
        CardGroupType Type { get; }
        
        string Name { get; }

        List<IButton> CardButtons { get; }
        CardZone CardZone { get; }

        bool Loading { get; }
    }

    /// <summary>
    /// A logical grouping of cards that contains a pool (cards avaiable for use by this card group) and (optionally) CardZone(s) that
    /// represent physical locations of instances of cards in the real world
    /// </summary>
    public class CardGroup : ViewModel, ICardGroup, INotifyPropertyChanged {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        private string _playerName = string.Empty;
        private ShowCardZoneButton _showCardZoneButton = null;
        private readonly IList<CardZone> _cardZones = new List<CardZone>();

        public CardGroup(CardGroupId id) {
            Type = id.GetSelectableType();
            Id = id;
            CardButtons = new List<IButton>();
            _cardZones = new List<CardZone>();

            _eventBus.SubscribeToCardInfoVisibilityChanged(CardInfoVisibilityChangedHandler);
            _eventBus.SubscribeToCardZoneVisibilityToggled(CardZoneVisibilityToggledHandler);
        }

        public CardGroupType Type { get; }

        public CardGroupId Id { get; }

        public string Name { 
            get {
                switch(Type) {
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
                    _eventBus.PublishCardGroupChanged(Id, value, ButtonImageAsBytes != null, value);
                }
            }
        }

        public string CardZoneName { get { return _cardZones.Any() ? _cardZones[0].Name : ""; } }

        public List<IButton> CardButtons { get; set; }
        
        private byte[] _buttonImageAsBytes { get; set; }
        /// <summary>
        /// Image associated with this card group
        /// </summary>
        public byte[] ButtonImageAsBytes {
            get => _buttonImageAsBytes;
            set {
                _buttonImageAsBytes = value;
                _eventBus.PublishCardGroupChanged(Id, Name, value != null, Name);
            }
        }

        /// <summary>
        /// Add a Card Zone to this Card Group
        /// </summary>
        /// <param name="cardZone">Card Zone to add</param>
        public void AddCardZone(CardZone cardZone) {
            cardZone.Buttons.CollectionChanged += (s, e) => CardZoneUpdated();
            cardZone.CardGroupId = Id;
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

        public IEnumerable<CardZone> CardZones { get { return from cardZone in _cardZones select cardZone; } }

        /// <summary>
        /// The first Card Zone assigned to this CardGroup. Will be default(CardZone) if no Card Zone is assigned
        /// </summary>
        public CardZone CardZone { get { return GetCardZone(0); } }

        private bool _showCardZoneButtons;
        /// <summary>
        /// Whether card zones should be displayed on the UI
        /// </summary>
        public bool ShowCardZoneButtons { 
            get => _showCardZoneButtons;
            set {
                _showCardZoneButtons = value;
                NotifyPropertyChanged(nameof(ShowCardZoneButtons));
            }
        }

        public bool Loading { get; internal set; }
        public IEnumerable<CardInfo> CardPool { get => from button in CardButtons.OfType<CardInfoButton>() select button.CardInfo; }

        /// <summary>
        /// Find a button
        /// </summary>
        /// <param name="context">Information to find the button</param>
        /// <returns>The button identified by the context- default if not found</returns>
        internal IButton GetButton(IButtonContext context) {
            if (context.CardGroupId != Id) {
                return default(Button);
            }

            return GetButton(context.ButtonMode, context.Index);
        }

        /// <summary>
        /// Find a button
        /// </summary>
        /// <param name="buttonMode">Look in the pool or in card zones</param>
        /// <param name="index">index of the button</param>
        /// <returns>The button identified by the parameters- default if not found</returns>
        internal IButton GetButton(ButtonMode buttonMode, int index) {
            if (buttonMode == ButtonMode.Pool) {
                return (index < CardButtons.Count) ? CardButtons[index] : default(Button);
            }

            if (CardZone == default(CardZone)) {
                return default(Button);
            }

            return index < CardZone.Buttons.Count ? CardZone.Buttons[index] : default(Button);
        }

        /// <summary>
        /// Remove all CardInfos from the pool and all card zones
        /// </summary>
        internal void ClearCards() {
            CardButtons.Clear();
            foreach (var cardZone in _cardZones) {
                cardZone.Buttons.Clear();
            }
            NotifyPropertyChanged(nameof(CardButtons));
        }

        internal void LoadCards(IEnumerable<CardInfo> cards) {
            var clearButton = new ClearButton();

            var playerButtons = new List<IButton> { clearButton };

            if (_cardZones.Count > 0) {
                _showCardZoneButton = new ShowCardZoneButton();
                playerButtons.Add(_showCardZoneButton);
                UpdateShowCardZoneButtonName();
            }

            playerButtons.AddRange(from card in SortCards(cards) select new CardInfoButton(card, Id));
            CardButtons = playerButtons;
            NotifyPropertyChanged(nameof(CardButtons));
        }

        /// <summary>
        /// Look through all card zones to find this button and remove it
        /// </summary>
        /// <param name="button">The button to remove</param>
        internal void RemoveCard(CardButton button) {
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
            //don't sort scenario cards- easier to find when acts/agendas are in order
            if (firstCard.Type == CardType.Scenario) {
                return cards;
            }

            var sortedCards = cards.OrderBy(x => x.Name.Replace("\"", "")).ToList();
            if (firstCard.Type == CardType.Location) {
                //for location cards, we want the backs before the front in the list
                for (var index = 0; index < sortedCards.Count(); index++) {
                    var card = sortedCards[index];
                    var flipSideCardIndex = sortedCards.IndexOf(card.FlipSideCard);
                    if (flipSideCardIndex > index) {
                        sortedCards.RemoveAt(flipSideCardIndex);
                        sortedCards.Insert(index, card.FlipSideCard);
                    }
                }
            }

            return sortedCards;
        }

        /// <summary>
        /// When card info visibility has changed, look through all of our buttons to see if we need to show that they are visible
        /// </summary>
        /// <param name="e">CardInfoVisibilityChanged</param>
        private void CardInfoVisibilityChangedHandler(CardInfoVisibilityChanged e) {
            var cardImageButtons = CardButtons.OfType<CardImageButton>();
            foreach (var cardZone in _cardZones) {
                cardImageButtons = cardImageButtons.Union(cardZone.Buttons.OfType<CardImageButton>());
            }

            foreach (var button in cardImageButtons) {
                if (e.Code == button.CardInfo.ImageId) {
                    button.IsToggled = e.IsVisible;
                }
            }
        }

        /// <summary>
        /// When the card zone for this card group has been toggled, update the card zone toggle button pressed state
        /// </summary>
        /// <param name="e">CardZoneVisibilityToggled</param>
        private void CardZoneVisibilityToggledHandler(CardZoneVisibilityToggled e) {
            var cardZone = e.CardZone;
            if (cardZone != CardZone) {
                return;
            }

            _showCardZoneButton.IsToggled = e.IsVisible;
            _eventBus.PublishButtonToggled(Id, ButtonMode.Pool, CardButtons.IndexOf(_showCardZoneButton), e.IsVisible);
        }


        private void CardZoneUpdated() {
            UpdateShowCardZoneButtonName();
            ShowCardZoneButtons = _cardZones[0].Buttons.Count > 0;
        }

        private void UpdateShowCardZoneButtonName() {
            if (_showCardZoneButton == null) {
                return;
            }

            if (_cardZones[0].Buttons.Any()) {
                _showCardZoneButton.Text = "Show " + _cardZones[0].Name + " (" + _cardZones[0].Buttons.Count + ")";
            } else {
                _showCardZoneButton.Text = "Right Click to add cards to " + _cardZones[0].Name;
            }
            _eventBus.PublishButtonTextChanged(Id, 0, CardButtons.IndexOf(_showCardZoneButton), _showCardZoneButton.Text);
        }
    }
}
