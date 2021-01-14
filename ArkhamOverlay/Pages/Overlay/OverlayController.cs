using ArkhamOverlay.Data;
using PageController;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace ArkhamOverlay.Pages.Overlay {
    public class OverlayController : Controller<OverlayView, OverlayViewModel> {
        private readonly AppData _appData;
        private readonly Configuration _configuration;

        public OverlayController(AppData appData) {
            _appData = appData;
            _configuration = appData.Configuration;

            _configuration.PropertyChanged += (s, e) => {
                if ((e.PropertyName == nameof(Configuration.OverlayHeight))
                || (e.PropertyName == nameof(Configuration.OverlayWidth))
                || (e.PropertyName == nameof(Configuration.CardHeight))
                || (e.PropertyName == nameof(Configuration.ActAgendaCardHeight))
                || (e.PropertyName == nameof(Configuration.HandCardHeight))) {
                    CalculateMaxHeightForCards();    
                }
            };

            foreach (var overlayCards in ViewModel.AllOverlayCards) {
                overlayCards.CollectionChanged += (s, e) => CalculateMaxHeightForCards();
            }

            ViewModel.AppData = appData;

            foreach (var selectableCards in appData.Game.AllSelectableCards) {
                InitializeSelectableCards(selectableCards);
            }

            View.Closed += (s, e) => {
                foreach (var overlayCards in ViewModel.AllOverlayCards) {
                    foreach (var overlayCard in overlayCards) {
                        overlayCard.Card.IsDisplayedOnOverlay = false;
                    }
                }

                Closed?.Invoke();
            };
        }

        private void CalculateMaxHeightForCards() {
            var actAgendaHeight = Math.Min(_configuration.ActAgendaCardHeight,  CalculateMaxHeightForRow(ViewModel.ActAgendaCards));
            var handHeight = Math.Min(_configuration.HandCardHeight, CalculateMaxHeightForRow(ViewModel.HandCards));
            var encounterHeight = Math.Min(_configuration.CardHeight, CalculateMaxHeightForRow(ViewModel.EncounterCards));
            var playerHeight = Math.Min(_configuration.CardHeight, CalculateMaxHeightForRow(ViewModel.PlayerCards));

            var margins = 50; // top and bottom
            if (actAgendaHeight > 0) margins += 10;
            if (handHeight > 0) margins += 10;
            if (encounterHeight > 0) margins += 10;
            if (playerHeight > 0) margins += 10;

            var overlayHeightWithMargins = _configuration.OverlayHeight - margins;

            if ((actAgendaHeight + handHeight + encounterHeight + playerHeight) > overlayHeightWithMargins) {
                if ((encounterHeight > 0) && (playerHeight > 0)) {
                    var cardMaxHeight = (overlayHeightWithMargins - actAgendaHeight - handHeight) / 2;
                    encounterHeight = Math.Min(cardMaxHeight, encounterHeight);
                    playerHeight = overlayHeightWithMargins - actAgendaHeight - handHeight - encounterHeight;
                } else {
                    encounterHeight = Math.Min(encounterHeight, overlayHeightWithMargins - actAgendaHeight - handHeight);
                    playerHeight = Math.Min(playerHeight, overlayHeightWithMargins - actAgendaHeight - handHeight);
                }
            }

            SetMaxHeightForRow(ViewModel.ActAgendaCards, actAgendaHeight);
            SetMaxHeightForRow(ViewModel.HandCards, handHeight);
            SetMaxHeightForRow(ViewModel.EncounterCards, encounterHeight);
            SetMaxHeightForRow(ViewModel.PlayerCards, playerHeight);
        }

        private double CalculateMaxHeightForRow(IList<OverlayCardViewModel> overlayCards) {
            if (!overlayCards.Any()) {
                return 0;
            }

            var margin = overlayCards.First().Margin * 2;
            var horizontalCount = overlayCards.Count(x => x.Card.IsHorizontal);
            var verticalCount = overlayCards.Count(x => !x.Card.IsHorizontal);
            var maxCardWidth = (_configuration.OverlayWidth - 35 - (margin * (horizontalCount + verticalCount) )) / (verticalCount + horizontalCount / OverlayCardViewModel.CardWidthRatio);

            return maxCardWidth / OverlayCardViewModel.CardWidthRatio;
        }

        private void SetMaxHeightForRow(IList<OverlayCardViewModel> overlayCards, double maxHeight) {
            foreach (var overlayCard in overlayCards) {
                overlayCard.MaxHeight = maxHeight;
            }
        }


        private void InitializeSelectableCards(SelectableCards selectableCards) {
            selectableCards.CardVisibilityToggled += ToggleCardVisibilityHandler;

            selectableCards.CardSet.VisibilityToggled += () => {
                if (selectableCards.Type == SelectableType.Scenario) {
                    ToggleActAgendaVisibility();
                } else {
                    ToggleHandVisibility(selectableCards.CardSet);
                }
            };

            selectableCards.CardSet.Buttons.CollectionChanged += (s, e) => {
                if (!selectableCards.CardSet.IsDisplayedOnOverlay) {
                    return;
                }

                if (selectableCards.Type == SelectableType.Scenario) {
                    UpdateCardSet(selectableCards.CardSet, ViewModel.ActAgendaCards);
                } else {
                    UpdateCardSet(selectableCards.CardSet, ViewModel.HandCards);
                }
            };
        }

        public double Top { get => View.Top; set => View.Top = value; }
        public double Left { get => View.Left; set => View.Left = value; }

        public void Close() {
            View.Close();
        }

        public void Activate() {
            View.Activate();
        }

        internal void Show() {
            View.Show();
        }

        public event Action Closed;

        internal void ToggleCardVisibilityHandler(Card card) {
            card.IsDisplayedOnOverlay = !card.IsDisplayedOnOverlay;

            var overlayCards = GetCardList(card);

            var overlayCard = overlayCards.FirstOrDefault(x => x.Card == card);
            if (overlayCard != null) {
                overlayCards.RemoveOverlayCards(overlayCard);
                return;
            }

            var newOverlayCard = new OverlayCardViewModel(ViewModel.AppData.Configuration, OverlayCardType.Display) { Card = card };

            var overlayCardToReplace = overlayCards.FindCardToReplace(card);
            if (overlayCardToReplace == null) {
                overlayCards.AddOverlayCard(newOverlayCard);
            } else {
                overlayCardToReplace.Card.IsDisplayedOnOverlay = false;
                overlayCards[overlayCards.IndexOf(overlayCardToReplace)] = newOverlayCard;
            }

        }

        private ObservableCollection<OverlayCardViewModel> GetCardList(Card card) {
            if (card.IsPlayerCard) {
                return ViewModel.PlayerCards;
            }

            return ViewModel.EncounterCards;
        }

        private void ToggleActAgendaVisibility() {
            var cardSet = _appData.Game.ScenarioCards.CardSet;
            if (cardSet.IsDisplayedOnOverlay) {
                ViewModel.ActAgendaCards.RemoveOverlayCards(ViewModel.ActAgendaCards.ToArray());
                cardSet.IsDisplayedOnOverlay = false;
                return;
            }

            cardSet.IsDisplayedOnOverlay = true;
            UpdateCardSet(cardSet, ViewModel.ActAgendaCards);
        }

        private void ToggleHandVisibility(CardSet cardSet) {
            //if there is a current hand being displayed- clear it
            var currentHandCardSet = ViewModel.CurrentlyDisplayedHandCardSet;
            if (currentHandCardSet != null) {
                currentHandCardSet.IsDisplayedOnOverlay = false;
                ViewModel.CurrentlyDisplayedHandCardSet = null;

                ViewModel.HandCards.RemoveOverlayCards(ViewModel.HandCards.ToArray());
            }

            //if this hand is the hand that was already set, all we were doing was hiding it
            if (cardSet == currentHandCardSet) {
                return;
            }

            ViewModel.CurrentlyDisplayedHandCardSet = cardSet;
            cardSet.IsDisplayedOnOverlay = true;
            UpdateCardSet(cardSet, ViewModel.HandCards);
        }

        private void UpdateCardSet(CardSet cardSet, ObservableCollection<OverlayCardViewModel> overlayCards) {
            var overlayCardsToRemove = new List<OverlayCardViewModel>();
            foreach (var overlayCard in overlayCards) {
                if (!cardSet.CardInstances.Contains(overlayCard.CardInstance)) {
                    overlayCardsToRemove.Add(overlayCard);
                }
            }
            if (overlayCardsToRemove.Any()) {
                overlayCards.RemoveOverlayCards(overlayCardsToRemove.ToArray());
            }

            foreach (var cardInstance in cardSet.CardInstances) {
                if (!overlayCards.Any(x => x.CardInstance == cardInstance)) {
                    var overlayCardType = (overlayCards == ViewModel.ActAgendaCards) ? OverlayCardType.ActAgenda : OverlayCardType.Hand;

                    var newOverlayCard = new OverlayCardViewModel(ViewModel.AppData.Configuration, overlayCardType) { CardInstance = cardInstance };
                    overlayCards.AddOverlayCard(newOverlayCard);
                }
            }
        }
    }

    public static class OverlayCardExtensions {
        public static void AddOverlayCard(this ObservableCollection<OverlayCardViewModel> cards, OverlayCardViewModel cardViewModel) {
            var insertIndex = cards.Count;

            if (cardViewModel.Card.Type == CardType.Agenda) {
                //add this directly to the left of the first act
                var firstAct = cards.FirstOrDefault(x => x.Card.Type == CardType.Act);
                if (firstAct != null) {
                    insertIndex = cards.IndexOf(firstAct);
                }
            }

            cards.Insert(insertIndex, cardViewModel);
        }

        public static void RemoveOverlayCards(this ObservableCollection<OverlayCardViewModel> overlayCards, params OverlayCardViewModel[] overlayCardsToRemove) {
            foreach (var overlayCard in overlayCardsToRemove) {
                overlayCards.Remove(overlayCard);
            }
        }

        public static OverlayCardViewModel FindCardToReplace(this ObservableCollection<OverlayCardViewModel> overlayCards, Card card) {
            return overlayCards.FirstOrDefault(x => x.Card == card.FlipSideCard);
        }
    }
}
