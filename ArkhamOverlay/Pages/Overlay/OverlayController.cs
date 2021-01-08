using ArkhamOverlay.CardButtons;
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
            ViewModel.AppData = appData;

            foreach (var selectableCards in appData.Game.AllSelectableCards) {
                InitializeSelectableCards(selectableCards);
            }

            View.Closed += (s, e) => {
                Closed?.Invoke();
            };
        }

        private void InitializeSelectableCards(SelectableCards selectableCards) {
            foreach (var cardButtons in selectableCards.CardButtons) {
                if (!(cardButtons is Card card)) {
                    continue;
                }

                if (card.IsVisible) {
                    ToggleCard(card, null);
                }
            }

            selectableCards.CardVisibilityToggled += ToggleCardVisibilityHandler;
            selectableCards.SetUpdated += SetUpdatedHandler;
        }

        private void MoveActAgendaCards(bool useActAgendaBar) {
            var sourceCards = ViewModel.ActAgendaCards;
            var destinationCards = ViewModel.EncounterCards;
            if (useActAgendaBar) {
                sourceCards = ViewModel.EncounterCards;
                destinationCards = ViewModel.ActAgendaCards;
            }

            var cardsToMove = new List<OverlayCardViewModel>();
            foreach (var cardViewModel in sourceCards) {
                if (cardViewModel.Card.Type == CardType.Act || cardViewModel.Card.Type == CardType.Agenda) {
                    cardsToMove.Add(cardViewModel);
                }
            }

            foreach (var cardToMove in cardsToMove) {
                sourceCards.Remove(cardToMove);
                destinationCards.Add(cardToMove);
            }
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

        internal void ToggleCardVisibilityHandler(Card card, Card cardToReplace) {
            if (Application.Current.Dispatcher.CheckAccess()) {
                ToggleCard(card, cardToReplace);
            } else {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                    ToggleCard(card, cardToReplace);
                }));
            }
        }

        internal void ToggleCard(Card card, Card cardToReplace) {
            var overlayCards = GetCardList(card);

            var existingCardViewModel = overlayCards.FirstOrDefault(x => x.Card == card);
            if (existingCardViewModel == null) {
                var newOverlayCard = new OverlayCardViewModel(ViewModel.AppData.Configuration) { Card = card };

                var overlayCardToReplace = cardToReplace != null ? overlayCards.FirstOrDefault(x => x.Card.Code == cardToReplace.Code) : null;
                if (overlayCardToReplace == null) {
                    overlayCards.AddOverlayCard(newOverlayCard);
                } else {
                    overlayCards[overlayCards.IndexOf(overlayCardToReplace)] = newOverlayCard;
                }
            } else {
                existingCardViewModel.Show = false;
                var dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += (s, e) => {
                    overlayCards.Remove(existingCardViewModel);
                    dispatcherTimer.Stop();
                };
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                dispatcherTimer.Start();
            }
        }

        private ObservableCollection<OverlayCardViewModel> GetCardList(Card card) {
            if (card.IsPlayerCard) {
                return ViewModel.PlayerCards;
            }

            return ViewModel.EncounterCards;
        }

        private void SetUpdatedHandler(SelectableCards selectableCards, bool isVisible) {
            var overlayCards = selectableCards.Type == SelectableType.Scenario ? ViewModel.ActAgendaCards : ViewModel.HandCards;
            if (!isVisible) {
                foreach (var overlayCard in overlayCards) {
                    overlayCard.Show = false;
                }

                var dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += (s, e) => {
                    overlayCards.Clear();
                    dispatcherTimer.Stop();
                };
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                dispatcherTimer.Start();
            } else {

                if (selectableCards.Type == SelectableType.Player && overlayCards.Any()) {
                    //in the case where the player cards list is currently populated, we need to notify it that it is no longer being shown
                    foreach (var selectablePlayerCards in _appData.Game.AllSelectableCards.Where(x => x.Type == SelectableType.Player && x != selectableCards)) {
                        selectablePlayerCards.SetHidden();
                    }
                }

                var cardSet = selectableCards.CardSet;
                var cardRemoved = false;
                //remove cards 
                foreach (var overlayCard in overlayCards) {
                    //copyindex = which copy of this card (first, second, third) are we dealing with
                    var copyIndex = overlayCards.Count(x => x.Card == overlayCard.Card && overlayCards.IndexOf(x) <= overlayCards.IndexOf(overlayCard));

                    //how many copies do we expect?
                    var expectedCount = cardSet.Count(x => x == overlayCard.Card);
                    if (copyIndex > expectedCount) {
                        overlayCard.Show = false;
                        cardRemoved = true;
                    }
                }

                //if any cards are removed- fade them out, then remove them
                if (cardRemoved) {
                    var dispatcherTimer = new DispatcherTimer();
                    dispatcherTimer.Tick += (s, e) => {
                        while (overlayCards.Any(x => !x.Show)) {
                            overlayCards.Remove(overlayCards.First(x => !x.Show));
                        }
                        AddMissingCardsFromSet(overlayCards, cardSet);
                        dispatcherTimer.Stop();
                    };
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                    dispatcherTimer.Start();
                } else {
                    AddMissingCardsFromSet(overlayCards, cardSet);
                }
            }
        }

        private void AddMissingCardsFromSet(ObservableCollection<OverlayCardViewModel> overlayCards, ObservableCollection<Card> cards) {
            foreach (var card in cards) {
                //how many cards should there be
                var expectedCount = cards.Count(x => x == card);

                //make it so
                while (overlayCards.Count(x => x.Card == card) < expectedCount) {
                    var newOverlayCard = new OverlayCardViewModel(ViewModel.AppData.Configuration) { Card = card };
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
    }
}
