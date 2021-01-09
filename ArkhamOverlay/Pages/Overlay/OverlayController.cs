﻿using ArkhamOverlay.Data;
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
            if (Application.Current.Dispatcher.CheckAccess()) {
                ToggleCard(card);
            } else {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    ToggleCard(card);
                }));
            }
        }

        internal void ToggleCard(Card card) {
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
            var cardsRemoved = false;
            if (currentHandCardSet != null) {
                cardsRemoved = true;
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

             if (cardsRemoved) { 
                //if this is a different hand, we need to show it- but we need to wait for the removal to complete to start showing new cards
                var dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += (s, e) => {
                    dispatcherTimer.Stop();
                    UpdateCardSet(cardSet, ViewModel.HandCards);
                };
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                dispatcherTimer.Start();
            } else {
                UpdateCardSet(cardSet, ViewModel.HandCards);
            }
        }

        private void UpdateCardSet(CardSet cardSet, ObservableCollection<OverlayCardViewModel> overlayCards) {
            var overlayCardsToRemove = new List<OverlayCardViewModel>();
            foreach (var overlayCard in overlayCards) {
                if (!cardSet.CardInstances.Contains(overlayCard.CardInstance)) {
                    overlayCardsToRemove.Add(overlayCard);
                }

                if (overlayCardsToRemove.Any()) {
                    overlayCards.RemoveOverlayCards(overlayCardsToRemove.ToArray());
                }
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
                overlayCard.Show = false;
                overlayCard.Card.IsDisplayedOnOverlay = false;
            }

            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (s, e) => {
                dispatcherTimer.Stop();
                foreach (var overlayCard in overlayCardsToRemove) {
                    overlayCards.Remove(overlayCard);
                }
            };
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            dispatcherTimer.Start();
        }

        public static OverlayCardViewModel FindCardToReplace(this ObservableCollection<OverlayCardViewModel> overlayCards, Card card) {
            return overlayCards.FirstOrDefault(x => x.Card == card.FlipSideCard);
        }
    }
}
