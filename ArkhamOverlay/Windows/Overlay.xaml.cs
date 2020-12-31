using ArkhamOverlay.Data;
using System.Linq;
using System.Windows;

namespace ArkhamOverlay {
    public partial class Overlay : Window {
        private OverlayData _overlayData;

        public Overlay() {
            InitializeComponent();
        }

        public void SetAppData(AppData appData) {
            _overlayData = new OverlayData {
                AppData = appData
            };

            var game = appData.Game;
            game.ScenarioCards.CardToggled += ToggleCard;
            game.ScenarioCards.ClearCards += ClearCards;

            game.LocationCards.CardToggled += ToggleCard;
            game.LocationCards.ClearCards += ClearCards;

            game.EncounterDeckCards.CardToggled += ToggleCard;
            game.EncounterDeckCards.ClearCards += ClearCards;

            foreach (var player in game.Players) {
                player.SelectableCards.CardToggled += ToggleCard;
                player.SelectableCards.ClearCards += ClearCards;
            }

            DataContext = _overlayData;
        }

        internal void ToggleCard(Card card) {
            var overlayCards = card.IsPlayerCard ? _overlayData.PlayerCards : _overlayData.EncounterCards;

            var existingOverlayCard = overlayCards.FirstOrDefault(x => x.Card == card);
            if (existingOverlayCard == null) {
                var newOverlayCard = new OverlayCard(_overlayData.Configuration) { Card = card };
                card.IsVisible = true;

                var overlayCardToReplace = overlayCards.FirstOrDefault(x => x.Card.Code == card.Code);
                if (overlayCardToReplace == null) {
                    overlayCards.Add(newOverlayCard);
                } else {
                    overlayCards[overlayCards.IndexOf(overlayCardToReplace)] = newOverlayCard;
                    overlayCardToReplace.Card.IsVisible = false;
                }
            } else {
                overlayCards.Remove(existingOverlayCard);
                existingOverlayCard.Card.IsVisible = false;
            }
        }

        internal void ClearAllCards() {
            foreach (var overlayCard in _overlayData.EncounterCards) {
                overlayCard.Card.IsVisible = false;
            }
            _overlayData.EncounterCards.Clear();

            foreach (var overlayCard in _overlayData.PlayerCards) {
                overlayCard.Card.IsVisible = false;
            }
            _overlayData.PlayerCards.Clear();
        }

        internal void ClearCards(SelectableType type, string id = null) {
            if(type == SelectableType.Player && !string.IsNullOrEmpty(id)) {
                _overlayData.ClearPlayerCards(id);
            } else if (type == SelectableType.Encounter) {
                _overlayData.ClearEncounterCards();
            } else if (type == SelectableType.Location) {
                _overlayData.ClearLocationCards();
            } else if (type == SelectableType.Scenario) {
                _overlayData.ClearScenarioCards();
            }
        }
    }
}
