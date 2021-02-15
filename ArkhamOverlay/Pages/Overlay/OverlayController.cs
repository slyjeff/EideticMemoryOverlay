using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Data;
using ArkhamOverlay.Utils;
using ArkhamOverlay.Services;
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
using System.Diagnostics;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Events;
using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Events;
using ArkhamOverlay.Common.Utils;

namespace ArkhamOverlay.Pages.Overlay {
    public class OverlayController : Controller<OverlayView, OverlayViewModel> {
        private readonly AppData _appData;
        private readonly LoggingService _logger;
        private readonly Configuration _configuration;
        private readonly ICardZoneManager _topCardZoneManager;
        private readonly ICardZoneManager _bottomCardZoneManager;
        private readonly IList<ICardZoneManager> _cardZoneManagers = new List<ICardZoneManager>();
        private readonly DispatcherTimer _autoSnapshotTimer = new DispatcherTimer();

        public OverlayController(AppData appData, LoggingService loggingService, IEventBus eventBus) {
            loggingService.LogMessage("Initializing Overlay Controller.");
            _appData = appData;
            _logger = loggingService;
            _configuration = appData.Configuration;
            _topCardZoneManager = CreateCardZoneManager(ViewModel.TopZoneCards);
            _bottomCardZoneManager = CreateCardZoneManager(ViewModel.BottomZoneCards);
            _autoSnapshotTimer.Tick += (e, s) => {
                AutoSnapshot();
            };
            _autoSnapshotTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _autoSnapshotTimer.IsEnabled = _configuration.UseAutoSnapshot;

            ViewModel.AppData = appData;

            eventBus.SubscribeToButtonInfoChanged(ButtonInfoChangedHandler);
            eventBus.SubscribeToButtonRemoved(ButtonRemovedHandler);
            eventBus.SubscribeToTakeSnapshotRequest(TakeSnapshotHandler);
            eventBus.SubscribeToShowDeckListRequest(ShowDeckListRequestHandler);
            eventBus.SubscribeToToggleCardVisibilityRequest(ToggleCardVisibilityRequestHandler);
            eventBus.SubscribeToClearAllCardsRequest(ClearAllCardsRequestHandler);
            eventBus.SubscribeToClearAllCardsForCardGroupRequest(ClearAllCardsForCardGroupRequestHandler);
            eventBus.SubscribeToToggleCardZoneVisibilityRequest(ToggleCardZoneVisibilityRequestHandler);

            _configuration.PropertyChanged += (s, e) => {
                if ((e.PropertyName == nameof(Configuration.OverlayHeight))
                || (e.PropertyName == nameof(Configuration.OverlayWidth))
                || (e.PropertyName == nameof(Configuration.CardHeight))
                || (e.PropertyName == nameof(Configuration.TopCardZoneHeight))
                || (e.PropertyName == nameof(Configuration.BottomCardZoneHeight))) {
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

            View.Closed += (s, e) => {
                eventBus.UnsubscribeFromButtonInfoChanged(ButtonInfoChangedHandler);
                eventBus.UnsubscribeFromButtonRemoved(ButtonRemovedHandler);
                eventBus.UnsubscribeFromShowDeckListRequest(ShowDeckListRequestHandler);
                eventBus.UnsubscribeFromTakeSnapshotRequest(TakeSnapshotHandler);
                eventBus.UnsubscribeFromToggleCardVisibilityRequest(ToggleCardVisibilityRequestHandler);
                eventBus.UnsubscribeFromClearAllCardsRequest(ClearAllCardsRequestHandler);
                eventBus.UnsubscribeFromClearAllCardsForCardGroupRequest(ClearAllCardsForCardGroupRequestHandler);
                eventBus.UnsubscribeFromToggleCardZoneVisibilityRequest(ToggleCardZoneVisibilityRequestHandler);

                _autoSnapshotTimer.Stop();
                foreach (var overlayCards in ViewModel.AllOverlayCards) {
                    foreach (var overlayCard in overlayCards) {
                        overlayCard.CardTemplate.IsDisplayedOnOverlay = false;
                    }
                }

                Closed?.Invoke();
            };

            _logger.LogMessage("Finished initializing Overlay Controller.");
        }

        public double Top { get => View.Top; set => View.Top = value; }
        public double Left { get => View.Left; set => View.Left = value; }

        public void Close() {
            _logger.LogMessage("Closing overlay window.");
            View.Close();
        }

        public void Activate() {
            _logger.LogMessage("Activating overlay window.");
            View.Activate();
        }

        internal void Show() {
            _logger.LogMessage("Showing overlay window.");
            View.Show();
        }

        public event Action Closed;

        #region Size Calculations

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
            double topZoneCardsHeight = Math.Min(_configuration.TopCardZoneHeight * OverlayCardViewModel.CardWidthRatio,  CalculateMaxHeightForRow(ViewModel.TopZoneCards));
            var encounterCardTemplatesHeight = Math.Min(_configuration.CardHeight, CalculateMaxHeightForRow(ViewModel.EncounterCardTemplates));
            var playerCardTemplatesHeight = Math.Min(_configuration.CardHeight, CalculateMaxHeightForRow(ViewModel.PlayerCardTemplates));
            var bottomZoneCardsHeight = Math.Min(_configuration.BottomCardZoneHeight, CalculateMaxHeightForRow(ViewModel.BottomZoneCards));

            var isTopZoneVisible = topZoneCardsHeight > 0;
            var isEncounterCardTemplatesVisible = encounterCardTemplatesHeight > 0;
            var isPlayerCardTemplatesVisible = playerCardTemplatesHeight > 0;
            var isBottomZoneVisible = bottomZoneCardsHeight > 0;

            var margins = 50; // top and bottom
            if (isTopZoneVisible) margins += 10;
            if (isEncounterCardTemplatesVisible) margins += 10;
            if (isPlayerCardTemplatesVisible) margins += 10;
            if (isBottomZoneVisible) margins += 10;

            var overlayHeightWithMargins = _configuration.OverlayHeight - margins;

            //is there enough room to show everything?
            if ((topZoneCardsHeight + bottomZoneCardsHeight + encounterCardTemplatesHeight + playerCardTemplatesHeight) > overlayHeightWithMargins) {
                //there is not- keep the act/agenda fixed and fill the remaining space
                var spaceToFill = overlayHeightWithMargins - topZoneCardsHeight;

                //how many zones do we have to account for?
                var remainingZoneCount = 0;
                if (isEncounterCardTemplatesVisible) remainingZoneCount++;
                if (isPlayerCardTemplatesVisible) remainingZoneCount++;
                if (isBottomZoneVisible) remainingZoneCount++;

                //what is the average remaining zone height?
                var averageHeight = spaceToFill / remainingZoneCount;

                //if the encounter height is smaller than the average height, we have extra we can add to the other zones
                var encounterHeightLeftover = (!isEncounterCardTemplatesVisible || encounterCardTemplatesHeight > averageHeight) ? 0 : averageHeight - encounterCardTemplatesHeight;

                //if the player height is smaller than the average height, we have extra we can add to the other zones
                var playerHeightLeftover = (!isPlayerCardTemplatesVisible || playerCardTemplatesHeight > averageHeight) ? 0 : averageHeight - playerCardTemplatesHeight;

                //if the hand height is smaller than the average heigh, we have extra we can add to the encounter player
                var handHeightLeftover = (!isBottomZoneVisible || bottomZoneCardsHeight > averageHeight) ? 0 : averageHeight - bottomZoneCardsHeight;
                
                //hand heightLeftover needs to be split if both the encounter and player zones are visible
                if (isEncounterCardTemplatesVisible && isPlayerCardTemplatesVisible) {
                    handHeightLeftover = handHeightLeftover / 2;
                }

                encounterCardTemplatesHeight = Math.Min(encounterCardTemplatesHeight, averageHeight + handHeightLeftover);
                playerCardTemplatesHeight = Math.Min(playerCardTemplatesHeight, averageHeight + handHeightLeftover);
                bottomZoneCardsHeight = Math.Min(bottomZoneCardsHeight, averageHeight + encounterHeightLeftover + playerHeightLeftover);
            }

            SetMaxHeightForRow(ViewModel.TopZoneCards, topZoneCardsHeight / OverlayCardViewModel.CardWidthRatio);
            SetMaxHeightForRow(ViewModel.EncounterCardTemplates, encounterCardTemplatesHeight);
            SetMaxHeightForRow(ViewModel.PlayerCardTemplates, playerCardTemplatesHeight);
            SetMaxHeightForRow(ViewModel.BottomZoneCards, bottomZoneCardsHeight);
        }

        private double CalculateMaxHeightForRow(IList<OverlayCardViewModel> overlayCards) {
            if (!overlayCards.Any()) {
                return 0;
            }

            var margin = overlayCards.First().Margin * 2;
            var horizontalCount = overlayCards.Count(x => x.CardTemplate.IsHorizontal);
            var verticalCount = overlayCards.Count(x => !x.CardTemplate.IsHorizontal);
            var maxCardWidth = (_configuration.OverlayWidth - 35 - (margin * (horizontalCount + verticalCount) )) / (verticalCount + horizontalCount / OverlayCardViewModel.CardWidthRatio);

            return maxCardWidth / OverlayCardViewModel.CardWidthRatio;
        }

        private void SetMaxHeightForRow(IList<OverlayCardViewModel> overlayCards, double maxHeight) {
            foreach (var overlayCard in overlayCards) {
                overlayCard.MaxHeight = maxHeight;
            }
        }

        #endregion

        #region CardZone Managers
        private ICardZoneManager CreateCardZoneManager(IList<OverlayCardViewModel> overlayCards) {
            var cardZoneManager = ServiceLocator.GetService<CardZoneManager>();
            cardZoneManager.SetOverlayCards(overlayCards);

            _cardZoneManagers.Add(cardZoneManager);

            return cardZoneManager;
        }

        private bool IsCardZoneVisible(CardZone cardZone) {
            return _cardZoneManagers.Any(x => x.IsShowingCardZone(cardZone));
        }
        #endregion

        private void ButtonInfoChangedHandler(ButtonInfoChanged e) {
            if (e.ButtonMode == ButtonMode.Zone) {
                UpdateCardZone(GetCardZoneFromCardGroupId(e.CardGroupId));
            }
        }

        private void ButtonRemovedHandler(ButtonRemoved e) {
            if (e.ButtonMode == ButtonMode.Zone) {
                UpdateCardZone(GetCardZoneFromCardGroupId(e.CardGroupId));
            }
        }

        private void UpdateCardZone(CardZone cardZone) {
            if (cardZone == default(CardZone)) {
                return;
            }

            if (!IsCardZoneVisible(cardZone)) {
                return;
            }

            var cardZoneManager = cardZone.Location == CardZoneLocation.Top ? _topCardZoneManager : _bottomCardZoneManager;
            cardZoneManager.Update(cardZone);
        }

        private CardZone GetCardZoneFromCardGroupId(CardGroupId cardGroupId) {
            foreach (var cardGroup in _appData.Game.AllCardGroups) {
                if (cardGroup.Id == cardGroupId) {
                    return cardGroup.CardZone;
                }
            }
            return null;
        }


        private CardGroup _currentDisplayedDeckList = null;
        private void ShowDeckListRequestHandler(ShowDeckListRequest request) {
            _logger.LogMessage("Showing deck list in overlay.");
            var selectableCards = GetCardGroupFromId(request.CardGroupId);

            //it's already displayed- just hide it
            if (_currentDisplayedDeckList == selectableCards) {
                ClearDeckList();
                return;
            }

            var cards = from cardButton in selectableCards.CardButtons.OfType<CardTemplateButton>()
                        select cardButton.CardTemplate;

            var deckList = new List<DeckListItem>();
            foreach (var card in cards.Where(c => !c.IsBonded)) {
                deckList.Add(new DeckListItem(card));
            }

            var bondedCards = cards.Where(c => c.IsBonded);
            if (bondedCards.Any()) {
                deckList.Add(new DeckListItem("Bonded Cards:"));

                foreach (var card in bondedCards) {
                    deckList.Add(new DeckListItem(card));
                }
            }

            _currentDisplayedDeckList = selectableCards;
            ViewModel.DeckList = deckList;
            ViewModel.ShowDeckList = true;
        }

        private void ClearDeckList() {
            _logger.LogMessage("Clearing deck list in overlay.");
            ViewModel.ShowDeckList = false;
            ViewModel.DeckList = null;
            _currentDisplayedDeckList = null;
        }

        internal void ToggleCardVisibilityRequestHandler(ToggleCardVisibilityRequest request) {
            var cardTemplate = request.CardTemplate;

            _logger.LogMessage($"Showing card {cardTemplate.Name} in overlay.");
            ClearDeckList();

            ToggleCardVisibility(cardTemplate);
        }

        private void ToggleCardVisibility(CardTemplate cardTemplate) {
            if (cardTemplate.IsDisplayedOnOverlay) {
                HideCardTemplate(cardTemplate);
            } else {
                ShowCardTemplate(cardTemplate);
            }
        }

        private void HideCardTemplate(CardTemplate cardTemplate) {
            var overlayCards = GetCardList(cardTemplate);
            var overlayCard = overlayCards.FirstOrDefault(x => x.CardTemplate == cardTemplate);
            if (overlayCard != null) {
                overlayCards.RemoveOverlayCards(overlayCard);
            }

            cardTemplate.IsDisplayedOnOverlay = false;
        }

        private void ShowCardTemplate(CardTemplate cardTemplate) {
            var overlayCards = GetCardList(cardTemplate);

            var newOverlayCard = new OverlayCardViewModel(ViewModel.AppData.Configuration, OverlayCardType.Template) { CardTemplate = cardTemplate };

            var overlayCardToReplace = overlayCards.FindCardTemplateToReplace(cardTemplate);
            if (overlayCardToReplace == null) {
                overlayCards.AddOverlayCard(newOverlayCard);
            } else {
                overlayCardToReplace.CardTemplate.IsDisplayedOnOverlay = false;
                overlayCards[overlayCards.IndexOf(overlayCardToReplace)] = newOverlayCard;
            }

            cardTemplate.IsDisplayedOnOverlay = true;
        }

        private ObservableCollection<OverlayCardViewModel> GetCardList(CardTemplate card) {
            if (card.IsPlayerCard) {
                return ViewModel.PlayerCardTemplates;
            }

            return ViewModel.EncounterCardTemplates;
        }

        private void ToggleCardZoneVisibilityRequestHandler(ToggleCardZoneVisibilityRequest request) {
            ClearDeckList();

            ToggleCardZone(request.CardZone);
        }

        private void ToggleCardZone(CardZone cardZone) {
            var cardZoneManager = cardZone.Location == CardZoneLocation.Top ? _topCardZoneManager : _bottomCardZoneManager;
            cardZoneManager.ToggleVisibility(cardZone);
        }

        private void ClearAllCardsForCardGroupRequestHandler(ClearAllCardsForCardGroupRequest request) {
            var cardGroup = request.CardGroup;
            var cardZone = cardGroup.CardZone;
            if (cardZone != default(CardZone)) {
                if (IsCardZoneVisible(cardZone)) {
                    ToggleCardZone(cardZone);
                }
            }

            foreach (var cardTemplate in cardGroup.CardPool) {
                if (cardTemplate.IsDisplayedOnOverlay) {
                    ToggleCardVisibility(cardTemplate);
                }
            }
        }

        private void ClearAllCardsRequestHandler(ClearAllCardsRequest request) {
            _topCardZoneManager.Clear();
            _bottomCardZoneManager.Clear();

            var allVisibleCardTemplates = ViewModel.EncounterCardTemplates.Union(ViewModel.PlayerCardTemplates).ToList();
            foreach (var overlayCard in allVisibleCardTemplates) {
                HideCardTemplate(overlayCard.CardTemplate);
            }
        }

        private void AutoSnapshot() {
            //every half second we write a snapshot to a temp file and then try to overwrite the configured file with it
            //this prevents OBS from getting in a weird state where it tries to read a file that is not there (because we are writing it)
            var tempFileName = Path.GetTempFileName();
            WriteSnapshotToFile(tempFileName);

            //only overwrite the overlay file if there has been a change
            if (FileCompare.CompareFiles(tempFileName, _appData.Configuration.AutoSnapshotFilePath)) {
                File.Delete(tempFileName);
                return;
            }

            try {
                File.Delete(_appData.Configuration.AutoSnapshotFilePath);
                File.Move(tempFileName, _appData.Configuration.AutoSnapshotFilePath);
            } catch (Exception ex) {
                //sometimes we can't write because obs is reading the file- just move on and get it next time
                _logger.LogException(ex, "Error writing auto snapshot to file.");
            }
        }

        private void TakeSnapshotHandler(TakeSnapshotRequest request) {
            _logger.LogMessage("Taking snapshot of overlay window.");

            var timeStamp = DateTime.Now.ToString("yyddMHHmmss");

            if (_appData.Configuration.SeperateStatSnapshots) {
                WriteSnapshotToFile($"{_appData.Game.SnapshotDirectory}\\OverlaySnapshot{timeStamp}.png", View.Cards);
                WriteSnapshotToFile($"{_appData.Game.SnapshotDirectory}\\OverlaySnapshot{timeStamp}-Stats.png", View.Stats);
            } else {
                WriteSnapshotToFile($"{_appData.Game.SnapshotDirectory}\\OverlaySnapshot{timeStamp}.png");
            }

            try {
                Process.Start(_appData.Game.SnapshotDirectory);
            } catch (Exception e) {
                _logger.LogException(e, "Error opening snapshot directory");
            }
        }

        private void WriteSnapshotToFile(string file, FrameworkElement controlToSnapshot = null) {
            if (controlToSnapshot == null) {
                controlToSnapshot = View.Overlay;
            }
            
            var renderTargetBitmap = new RenderTargetBitmap(Convert.ToInt32(controlToSnapshot.ActualWidth), Convert.ToInt32(controlToSnapshot.ActualHeight), 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(controlToSnapshot);
            var pngImage = new PngBitmapEncoder();
            pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            try {
                using (var fileStream = File.Create(file)) {
                    pngImage.Save(fileStream);
                }
            } catch (Exception ex) {
                //recover from a failure to write without crashing- we'll get it next time
                _logger.LogException(ex, "Error writing snapshot to file.");
            }
        }

        private CardGroup GetCardGroupFromId(CardGroupId id) {
            switch (id) {
                case CardGroupId.Player1:
                    return _appData.Game.Players[0].CardGroup;
                case CardGroupId.Player2:
                    return _appData.Game.Players[1].CardGroup;
                case CardGroupId.Player3:
                    return _appData.Game.Players[2].CardGroup;
                case CardGroupId.Player4:
                    return _appData.Game.Players[3].CardGroup;
                case CardGroupId.Scenario:
                    return _appData.Game.ScenarioCards;
                case CardGroupId.Locations:
                    return _appData.Game.LocationCards;
                case CardGroupId.EncounterDeck:
                    return _appData.Game.EncounterDeckCards;
                default:
                    return null;
            }
        }
    }
}
