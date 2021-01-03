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
        private Configuration _configuration;

        public OverlayController(AppData appData) {
            _configuration = appData.Configuration;
            ViewModel.Configuration = appData.Configuration;

            foreach (var selectableCards in appData.Game.AllSelectableCards) {
                InitializeSelectableCards(selectableCards);
            }

            var config = appData.Configuration;
            config.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(Configuration.UseActAgendaBar)) {
                    MoveActAgendaCards(config.UseActAgendaBar);
                }
            };

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

            selectableCards.CardToggled += ToggleCardHandler;
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

        internal void ToggleCardHandler(Card card, Card cardToReplace) {
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
                var newCardViewModel = new OverlayCardViewModel(ViewModel.Configuration) { Card = card };

                var overlayCardToReplace = cardToReplace != null ? overlayCards.FirstOrDefault(x => x.Card.Code == cardToReplace.Code) : null;
                if (overlayCardToReplace == null) {
                    overlayCards.AddOverlayCard(newCardViewModel);
                } else {
                    overlayCards[overlayCards.IndexOf(overlayCardToReplace)] = newCardViewModel;
                }
            } else {
                overlayCards.Remove(existingCardViewModel);
            }
        }


        private ObservableCollection<OverlayCardViewModel> GetCardList(Card card) {
            if (card.IsPlayerCard) {
                return ViewModel.PlayerCards;
            }

            if (_configuration.UseActAgendaBar) {
                if (card.Type == CardType.Act || card.Type == CardType.Agenda) {
                    return ViewModel.ActAgendaCards;
                }
            }

            return ViewModel.EncounterCards;
        }

        internal void ToggleActAgendaBar() {
            ViewModel.ShowActAgendaBar = !ViewModel.ShowActAgendaBar;
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
