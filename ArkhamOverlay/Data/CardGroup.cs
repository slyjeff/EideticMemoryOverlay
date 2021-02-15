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
        CardGroupType Type { get; }

        string Name { get; }

        List<IButton> CardButtons { get; }
        
        bool Loading { get; }
    }

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

            _eventBus.SubscribeToCardTemplateVisibilityChanged(CardTemplateVisibilityChanged);
            _eventBus.SubscribeToCardZoneVisibilityToggled(CardZoneVisibilityToggled);
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
                }
            }
        }

        public string CardZoneName { get { return _cardZones.Any() ? _cardZones[0].Name : ""; } }

        public List<IButton> CardButtons { get; set; }

        public void AddCardZone(CardZone cardZone) {
            cardZone.Buttons.CollectionChanged += (s, e) => CardZoneUpdated();
            cardZone.CardGroupId = Id;
            _cardZones.Add(cardZone);
        }

        public CardZone GetCardZone(int index) {
            if (index >= _cardZones.Count) {
                return default;
            }

            return _cardZones[index];
        }

        public CardZone CardZone { get { return GetCardZone(0); } }

        private bool _showCardZoneButtons;
        public bool ShowCardZoneButtons { 
            get => _showCardZoneButtons;
            set {
                _showCardZoneButtons = value;
                NotifyPropertyChanged(nameof(ShowCardZoneButtons));
            }
        }

        public bool Loading { get; internal set; }
        public IEnumerable<CardTemplate> CardPool { get => from button in CardButtons.OfType<CardTemplateButton>() select button.CardTemplate; }

        internal void ToggleCardZoneVisibility() {
            if (_showCardZoneButton == null) {
                return;
            }

            _showCardZoneButton.LeftClick();
        }

        /// <summary>
        /// Remove all CardTemplates from the pool and all card zones
        /// </summary>
        internal void ClearCards() {
            CardButtons.Clear();
            foreach (var cardZone in _cardZones) {
                cardZone.Buttons.Clear();
            }
            NotifyPropertyChanged(nameof(CardButtons));
        }

        internal void LoadCards(IEnumerable<CardTemplate> cards) {
            var clearButton = new ClearButton(this);

            var playerButtons = new List<IButton> { clearButton };

            if (_cardZones.Count > 0) {
                _showCardZoneButton = new ShowCardZoneButton(this.CardZone);
                playerButtons.Add(_showCardZoneButton);
                UpdateShowCardZoneButtonName();
            }

            playerButtons.AddRange(from card in SortCards(cards) select new CardTemplateButton(this, card));
            CardButtons = playerButtons;
            NotifyPropertyChanged(nameof(CardButtons));
        }

        private IEnumerable<CardTemplate> SortCards(IEnumerable<CardTemplate> cards) {
            var firstCard = cards.FirstOrDefault();
            if (firstCard == null) {
                return cards;
            }

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
        /// When card temlate visibility has changed, look through all of our buttons to see if we need to show that they are visible
        /// </summary>
        /// <param name="e">CardTemplateVisibilityChanged</param>
        private void CardTemplateVisibilityChanged(CardTemplateVisibilityChanged e) {
            var cardImageButtons = CardButtons.OfType<CardImageButton>();
            foreach (var cardZone in _cardZones) {
                cardImageButtons = cardImageButtons.Union(cardZone.Buttons.OfType<CardImageButton>());
            }

            foreach (var button in cardImageButtons) {
                if (e.Name == button.CardTemplate.Name) {
                    button.IsToggled = e.IsVisible;
                }
            }
        }

        /// <summary>
        /// When the card zone for this card group has been toggled, update the card zone toggle button pressed state
        /// </summary>
        /// <param name="e">CardZoneVisibilityToggled</param>
        private void CardZoneVisibilityToggled(CardZoneVisibilityToggled e) {
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
