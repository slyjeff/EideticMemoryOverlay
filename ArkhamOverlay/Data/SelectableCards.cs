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
    public interface ISelectableCards {
        SelectableType Type { get; }

        string Name { get; }

        List<IButton> CardButtons { get; }
        
        bool Loading { get; }
    }

    public class SelectableCards : ViewModel, ISelectableCards, INotifyPropertyChanged {
        private readonly IEventBus _eventBus = ServiceLocator.GetService<IEventBus>();
        private string _playerName = string.Empty;
        private ShowCardZoneButton _showCardZoneButton = null;

        public SelectableCards(CardGroup cardGroup) {
            Type = cardGroup.GetSelectableType();
            CardGroup = cardGroup;
            CardButtons = new List<IButton>();
            CardZone = new CardZone(this);
            CardZone.Buttons.CollectionChanged += (s, e) => CardZoneUpdated();

            _eventBus.SubscribeToCardTemplateVisibilityChanged(CardTemplateVisibilityChanged);
        }

        public SelectableType Type { get; }

        public CardGroup CardGroup { get; }

        public string Name { 
            get {
                switch(Type) {
                    case SelectableType.Scenario:
                        return "Act/Agenda/Scenario Reference";
                    case SelectableType.Location:
                        return "Location";
                    case SelectableType.Encounter:
                        return "Encounter Deck";
                    case SelectableType.Player:
                        return _playerName;
                    default:
                        return "Unknown";
                }
            }
            set {
                if (Type == SelectableType.Player) {
                    _playerName = value;
                }
            }
        }

        public string CardZoneName {
            get {
                switch (Type) {
                    case SelectableType.Scenario:
                        return "Act/Agena Bar:";
                    case SelectableType.Player:
                        return "In Hand:";
                    default:
                        return "";
                }
            }
        }

        public List<IButton> CardButtons { get; set; }
        public CardZone CardZone { get; }
        
        private bool _showCardZoneButtons;
        public bool ShowCardZoneButtons { 
            get => _showCardZoneButtons;
            set {
                _showCardZoneButtons = value;
                NotifyPropertyChanged(nameof(ShowCardZoneButtons));
            }
        }

        public bool Loading { get; internal set; }

        internal void ToggleCardZoneVisibility() {
            if (_showCardZoneButton == null) {
                return;
            }

            _showCardZoneButton.LeftClick();
        }


        internal void HideAllCards() {
            foreach (var showCardButton in CardButtons.OfType<CardTemplateButton>()) {
                if (showCardButton.CardTemplate.IsDisplayedOnOverlay) {
                    _eventBus.PublishToggleCardVisibilityRequest(showCardButton.CardTemplate);
                }
            }
        }

        private void CardZoneUpdated() {
            UpdateShowCardZoneButtonName();
            ShowCardZoneButtons = CardZone.Buttons.Count > 0;
        }

        private void UpdateShowCardZoneButtonName() {
            if (_showCardZoneButton == null) {
                return;
            }

            var buttonName = (Type == SelectableType.Scenario)
                ? "Act/Agenda Bar"
                : "Hand";

            if (CardZone.Buttons.Any()) {
                _showCardZoneButton.Text = "Show " + buttonName + " (" + CardZone.Buttons.Count + ")";
            } else {
                _showCardZoneButton.Text = "Right Click to add cards to " + buttonName;
            }
            _eventBus.PublishButtonTextChanged(CardGroup, 0, CardButtons.IndexOf(_showCardZoneButton), _showCardZoneButton.Text);
        }


        internal void LoadCards(IEnumerable<CardTemplate> cards) {
            var clearButton = new ClearButton(this);

            var playerButtons = new List<IButton> { clearButton };

            if (Type == SelectableType.Scenario || Type == SelectableType.Player) {
                _showCardZoneButton = new ShowCardZoneButton(this);
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

        internal void ClearCards() {
            HideAllCards();
            CardButtons.Clear();
            NotifyPropertyChanged(nameof(CardButtons));
        }

        /// <summary>
        /// When card temlate visibility has changed, look through all of our buttons to see if we need to show that they are visible
        /// </summary>
        /// <param name="e">CardTemplateVisibilityChangedEvent</param>
        private void CardTemplateVisibilityChanged(CardTemplateVisibilityChanged e) {
            var cardImageButtons = CardButtons.OfType<CardImageButton>().Union(CardZone.Buttons.OfType<CardImageButton>());

            foreach (var button in cardImageButtons) {
                if (e.Name == button.CardTemplate.Name) {
                    button.IsToggled = e.IsVisible;
                }
            }
        }
    }
}
