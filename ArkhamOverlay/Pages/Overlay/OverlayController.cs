using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Data;
using PageController;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ArkhamOverlay.Pages.Overlay {
    public class OverlayController : Controller<OverlayView, OverlayViewModel> {
        private readonly AppData _appData;
        private readonly Configuration _configuration;
        private readonly DispatcherTimer _autoSnapshotTimer = new DispatcherTimer();

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

        public OverlayController(AppData appData) {
            ViewModel.AppData = appData;
            _appData = appData;
            _configuration = appData.Configuration;
            _autoSnapshotTimer.Tick += (e, s) => {
                AutoSnapshot();
            };
            _autoSnapshotTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _autoSnapshotTimer.IsEnabled = _configuration.UseAutoSnapshot;

            _configuration.PropertyChanged += (s, e) => {
                if ((e.PropertyName == nameof(Configuration.OverlayHeight))
                || (e.PropertyName == nameof(Configuration.OverlayWidth))
                || (e.PropertyName == nameof(Configuration.CardHeight))
                || (e.PropertyName == nameof(Configuration.ActAgendaCardHeight))
                || (e.PropertyName == nameof(Configuration.HandCardHeight))) {
                    CalculateMaxHeightForCards();
                }

                if (e.PropertyName == nameof(Configuration.OverlayHeight)) {
                    CalculateDeckListHeight();
                    CalculateOverlayFontSize();
                    CalculateStatImageSize();
                }

                if (e.PropertyName == nameof(Configuration.OverlayWidth)) {
                    CalculateDeckListItemWidth();
                    CalculateDeckListMargin();
                }

                if (e.PropertyName == nameof(Configuration.UseAutoSnapshot)) {
                    _autoSnapshotTimer.IsEnabled = _configuration.UseAutoSnapshot;
                };
            };

            CalculateDeckListHeight();
            CalculateOverlayFontSize();
            CalculateStatImageSize();
            CalculateDeckListItemWidth();
            CalculateDeckListMargin();

            foreach (var overlayCards in ViewModel.AllOverlayCards) {
                overlayCards.CollectionChanged += (s, e) => CalculateMaxHeightForCards();
            }

            foreach (var selectableCards in appData.Game.AllSelectableCards) {
                InitializeSelectableCards(selectableCards);
            }

            View.Closed += (s, e) => {
                _autoSnapshotTimer.Stop();
                foreach (var overlayCards in ViewModel.AllOverlayCards) {
                    foreach (var overlayCard in overlayCards) {
                        overlayCard.Card.IsDisplayedOnOverlay = false;
                    }
                }

                Closed?.Invoke();
            };
        }

        private void CalculateDeckListHeight() {
            ViewModel.DeckListHeight = _appData.Configuration.OverlayHeight - 40;
        }

        private void CalculateOverlayFontSize() {
            ViewModel.OverlayFontSize = _appData.Configuration.OverlayHeight / 24;
        }

        private void CalculateStatImageSize() {
            ViewModel.StatImageSize = _appData.Configuration.OverlayHeight / 18;
        }

        private void CalculateDeckListItemWidth() {
            ViewModel.DeckListItemWidth = (_appData.Configuration.OverlayWidth - _appData.Configuration.OverlayWidth * .2) / 2;
        }

        private void CalculateDeckListMargin() {
            var horizontalMargin = _appData.Configuration.OverlayWidth * .1;

            ViewModel.DeckListMargin = new Thickness(horizontalMargin, 10, horizontalMargin, 10);
        }

        private void CalculateMaxHeightForCards() {
            var actAgendaHeight = Math.Min(_configuration.ActAgendaCardHeight * OverlayCardViewModel.CardWidthRatio,  CalculateMaxHeightForRow(ViewModel.ActAgendaCards));
            var handHeight = Math.Min(_configuration.HandCardHeight, CalculateMaxHeightForRow(ViewModel.HandCards));
            var encounterHeight = Math.Min(_configuration.CardHeight, CalculateMaxHeightForRow(ViewModel.EncounterCards));
            var playerHeight = Math.Min(_configuration.CardHeight, CalculateMaxHeightForRow(ViewModel.PlayerCards));

            var isActAgendaVisible = actAgendaHeight > 0;
            var isEncounterVisible = encounterHeight > 0;
            var isPlayerVisible = playerHeight > 0;
            var isHandVisible = handHeight > 0;

            var margins = 50; // top and bottom
            if (isActAgendaVisible) margins += 10;
            if (isEncounterVisible) margins += 10;
            if (isPlayerVisible) margins += 10;
            if (isHandVisible) margins += 10;

            var overlayHeightWithMargins = _configuration.OverlayHeight - margins;

            //is there enough room to show everything?
            if ((actAgendaHeight + handHeight + encounterHeight + playerHeight) > overlayHeightWithMargins) {
                //there is not- keep the act/agenda fixed and fill the remaining space
                var spaceToFill = overlayHeightWithMargins - actAgendaHeight;

                //how many zones do we have to account for?
                var remainingZoneCount = 0;
                if (isEncounterVisible) remainingZoneCount++;
                if (isPlayerVisible) remainingZoneCount++;
                if (isHandVisible) remainingZoneCount++;

                //what is the average remaining zone height?
                var averageHeight = spaceToFill / remainingZoneCount;

                //if the encounter height is smaller than the average height, we have extra we can add to the other zones
                var encounterHeightLeftover = (!isEncounterVisible || encounterHeight > averageHeight) ? 0 : averageHeight - encounterHeight;

                //if the player height is smaller than the average height, we have extra we can add to the other zones
                var playerHeightLeftover = (!isPlayerVisible || playerHeight > averageHeight) ? 0 : averageHeight - playerHeight;

                //if the hand height is smaller than the average heigh, we have extra we can add to the encounter player
                var handHeightLeftover = (!isHandVisible || handHeight > averageHeight) ? 0 : averageHeight - handHeight;
                
                //hand heightLeftover needs to be split if both the encounter and player zones are visible
                if (isEncounterVisible && isPlayerVisible) {
                    handHeightLeftover = handHeightLeftover / 2;
                }

                encounterHeight = Math.Min(encounterHeight, averageHeight + handHeightLeftover);
                playerHeight = Math.Min(playerHeight, averageHeight + handHeightLeftover);
                handHeight = Math.Min(handHeight, averageHeight + encounterHeightLeftover + playerHeightLeftover);
            }

            SetMaxHeightForRow(ViewModel.ActAgendaCards, actAgendaHeight / OverlayCardViewModel.CardWidthRatio);
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
            selectableCards.ShowDeckListTriggered += () => ShowDeckListHandler(selectableCards);
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

        private SelectableCards _currentDisplayedDeckList = null;
        private void ShowDeckListHandler(SelectableCards selectableCards) {
            //it's already displayed- just hide it
            if (_currentDisplayedDeckList == selectableCards) {
                ClearDeckList();
                return;
            }

            var cards = from cardButton in selectableCards.CardButtons.OfType<ShowCardButton>()
                        select cardButton.Card;

            var deckList = new List<DeckListItem>();
            foreach (var card in cards) {
                deckList.Add(new DeckListItem(card));
            }

            _currentDisplayedDeckList = selectableCards;
            ViewModel.DeckList = deckList;
            ViewModel.ShowDeckList = true;
        }

        private void ClearDeckList() {
            ViewModel.ShowDeckList = false;
            ViewModel.DeckList = null;
            _currentDisplayedDeckList = null;
        }

        internal void ToggleCardVisibilityHandler(Card card) {
            ClearDeckList();

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
            ClearDeckList();

            var cardSet = _appData.Game.ScenarioCards.CardSet;
            if (cardSet.IsDisplayedOnOverlay) {
                ViewModel.ActAgendaCards.RemoveOverlayCards(ViewModel.ActAgendaCards.ToArray());
                cardSet.IsDisplayedOnOverlay = false;
                return;
            }

            cardSet.IsDisplayedOnOverlay = true;
            UpdateCardSet(cardSet, ViewModel.ActAgendaCards);
        }


        private CardSet _currentlyDisplayedHandCardSet;
        private void ToggleHandVisibility(CardSet cardSet) {
            ClearDeckList();

            //if there is a current hand being displayed- clear it
            var currentHandCardSet = _currentlyDisplayedHandCardSet;

            ClearCurrentlyDisplayedHandCardSet();

            //if this hand is the hand that was already set, all we were doing was hiding it
            if (cardSet == currentHandCardSet) {
                return;
            }

            _currentlyDisplayedHandCardSet = cardSet;
            cardSet.IsDisplayedOnOverlay = true;
            UpdateCardSet(cardSet, ViewModel.HandCards);
        }

        private void ClearCurrentlyDisplayedHandCardSet() {
            if (_currentlyDisplayedHandCardSet == null) {
                return;
            }

            _currentlyDisplayedHandCardSet.IsDisplayedOnOverlay = false;
            _currentlyDisplayedHandCardSet = null;

            ViewModel.HandCards.RemoveOverlayCards(ViewModel.HandCards.ToArray());
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

        private void AutoSnapshot() {
            //every half second we write a snapshot to a temp file and then try to overwrite the configured file with it
            //this prevents OBS from getting in a weird state where it tries to read a file that is not there (because we are writing it)
            var tempFileName = Path.GetTempFileName();
            WriteSnapshotToFile(tempFileName);

            try {
                File.Delete(_appData.Configuration.AutoSnapshotFilePath);
                File.Move(tempFileName, _appData.Configuration.AutoSnapshotFilePath);
            } catch {
                //sometimes we can't write because obs is reading the file- just move on and get it next time
            }
        }

        internal void TakeSnapshot() {
            var fileName = "OverlaySnapshot" + DateTime.Now.ToString("yyddMHHmmss") + ".png";
            WriteSnapshotToFile(fileName);
        }

        private void WriteSnapshotToFile(string file) {
            var overlay = View.Overlay;
            var renderTargetBitmap = new RenderTargetBitmap(Convert.ToInt32(overlay.Width), Convert.ToInt32(overlay.Height), 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(overlay);
            var pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using (var fileStream = File.Create(file)) {
                pngImage.Save(fileStream);
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
