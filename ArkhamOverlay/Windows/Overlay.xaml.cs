using ArkhamOverlay.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

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
            foreach (var selectableCards in game.AllSelectableCards) {
                InitializeSelectableCards(selectableCards);
            }

            DataContext = _overlayData;
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
            var overlayCards = card.IsPlayerCard ? _overlayData.PlayerCards : _overlayData.EncounterCards;

            var existingOverlayCard = overlayCards.FirstOrDefault(x => x.Card == card);
            if (existingOverlayCard == null) {
                var newOverlayCard = new OverlayCard(_overlayData.Configuration) { Card = card };

                var overlayCardToReplace = cardToReplace != null ? overlayCards.FirstOrDefault(x => x.Card.Code == cardToReplace.Code) : null;
                if (overlayCardToReplace == null) {
                    overlayCards.AddOverlayCard(newOverlayCard);
                } else {
                    overlayCards[overlayCards.IndexOf(overlayCardToReplace)] = newOverlayCard;
                }
            } else {
                overlayCards.Remove(existingOverlayCard);
            }
        }
    }

    public static class OverlayCardExtensions {
        public static void AddOverlayCard(this ObservableCollection<OverlayCard> cards, OverlayCard overlayCard) {
            var insertIndex = cards.Count;

            if (overlayCard.Card.Type == CardType.Agenda) {
                //add this directly to the left of the first act
                var firstAct = cards.FirstOrDefault(x => x.Card.Type == CardType.Act);
                if (firstAct != null) {
                    insertIndex = cards.IndexOf(firstAct);
                }
            }

            cards.Insert(insertIndex, overlayCard);
        }
    }
}
