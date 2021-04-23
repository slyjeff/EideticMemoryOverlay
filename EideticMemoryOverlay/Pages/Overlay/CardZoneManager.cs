using EideticMemoryOverlay.PluginApi;
using Emo.Common.Services;
using Emo.Data;
using Emo.Events;
using Emo.Services;
using System.Collections.Generic;
using System.Linq;

namespace Emo.Pages.Overlay {
    public interface ICardZoneManager {
        void SetOverlayCards(IList<OverlayCardViewModel> overlayCards);
        bool IsShowingCardZone(CardZone cardZone);
        void ToggleVisibility(CardZone cardZone);
        void Update(CardZone cardZone);
        void Clear();
    }

    public class CardZoneManager : ICardZoneManager {
        private IList<OverlayCardViewModel> _overlayCards;
        private readonly Configuration _configuration;
        private readonly LoggingService _logger;
        private readonly IEventBus _eventBus;
        private CardZone _currentlyDisplayedCardZone;

        public CardZoneManager(Configuration configuration, LoggingService loggingService, IEventBus eventBus) {
            _configuration = configuration;
            _logger = loggingService;
            _eventBus = eventBus;
        }

        public void SetOverlayCards(IList<OverlayCardViewModel> overlayCards) {
            _overlayCards = overlayCards;
        }

        public bool IsShowingCardZone(CardZone cardZone) {
            return (cardZone == _currentlyDisplayedCardZone);
        }


        public void Update(CardZone cardZone) {
            _logger.LogMessage($"Updating {_currentlyDisplayedCardZone.Name} in overlay.");

            if (!IsShowingCardZone(cardZone)) {
                ClearCurrentlyDisplayedCardZone();
            }

            RemoveOverlayCardsNotInZone(cardZone);
            AddNewOverlayCards(cardZone);
        }

        public void ToggleVisibility(CardZone cardZoneToToggle) {
            //if there is a current hand being displayed- clear it
            var isHidingCurrentCardZone = IsShowingCardZone(cardZoneToToggle); 
            ClearCurrentlyDisplayedCardZone();

            //if the zone we are toggling was the one already displayed, we are done
            if (isHidingCurrentCardZone) {
                return;
            }

            _logger.LogMessage($"Showing {cardZoneToToggle.Name} in overlay.");
            AddNewOverlayCards(cardZoneToToggle);
        }

        public void Clear() {
            ClearCurrentlyDisplayedCardZone();
        }

        private void ClearCurrentlyDisplayedCardZone() {
            if (_currentlyDisplayedCardZone == null) {
                return;
            }

            _logger.LogMessage($"Hiding {_currentlyDisplayedCardZone.Name} in overlay.");
            _currentlyDisplayedCardZone.IsDisplayedOnOverlay = false;
            _currentlyDisplayedCardZone = null;

            _overlayCards.RemoveOverlayCards(_overlayCards.ToArray());
        }

        private void RemoveOverlayCardsNotInZone(CardZone cardZone) {
            var overlayCardsToRemove = new List<OverlayCardViewModel>();
            foreach (var overlayCard in _overlayCards) {
                if (!cardZone.Cards.Contains(overlayCard.Card)) {
                    overlayCardsToRemove.Add(overlayCard);
                }
            }
            _overlayCards.RemoveOverlayCards(overlayCardsToRemove.ToArray());
        }

        private void AddNewOverlayCards(CardZone cardZone) {
            foreach (var card in cardZone.Cards) {
                if (!_overlayCards.Any(x => x.Card == card)) {
                    AddCard(cardZone, card);
                }
            }

            _currentlyDisplayedCardZone = cardZone;
            _currentlyDisplayedCardZone.IsDisplayedOnOverlay = true;
        }

        private void AddCard(CardZone cardZone, ICard card) {
            var overlayCardType = (cardZone.Location == CardZoneLocation.Top) ? OverlayCardType.TopCardZone : OverlayCardType.BottomCardZone;

            var newOverlayCard = new OverlayCardViewModel(_configuration, overlayCardType) { Card = card };
            _overlayCards.AddOverlayCard(newOverlayCard);
        }
    }
}
