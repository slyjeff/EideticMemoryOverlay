using ArkhamOverlay.Data;
using ArkhamOverlay.Pages.Main;
using ArkhamOverlay.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace ArkhamOverlay {
    public partial class Overlay : Window {
        private OverlayViewModel _overlayViewModel;
        private Configuration _configuration;

        public Overlay() {
            InitializeComponent();
        }

        public void SetAppData(AppData appData) {
            _overlayViewModel = new OverlayViewModel(appData);
            _configuration = appData.Configuration;

            var game = appData.Game;
            foreach (var selectableCards in game.AllSelectableCards) {
                InitializeSelectableCards(selectableCards);
            }

            DataContext = _overlayViewModel;
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
                var newCardViewModel = new CardViewModel(_overlayViewModel.Configuration) { Card = card };

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

        private ObservableCollection<CardViewModel> GetCardList(Card card) {
            if (card.IsPlayerCard) {
                return _overlayViewModel.PlayerCards;
            }

            if (_configuration.UseActAgendaBar) {
                if (card.Type == CardType.Act || card.Type == CardType.Agenda) {
                    return _overlayViewModel.ActAgendaCards;
                }
            }

            return _overlayViewModel.EncounterCards;
        }

        internal void ToggleActAgendaBar() {
            _overlayViewModel.ShowActAgendaBar = !_overlayViewModel.ShowActAgendaBar;
        }
    }

    public static class OverlayCardExtensions {
        public static void AddOverlayCard(this ObservableCollection<CardViewModel> cards, CardViewModel cardViewModel) {
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
