using Emo.Data;
using Emo.Utils;
using Emo.Services;
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
using Emo.Common.Services;
using Emo.Common.Events;
using Emo.Common.Enums;
using Emo.Events;
using Emo.Common.Utils;
using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.Buttons;

namespace Emo.Pages.Overlay {
    public class OverlayController : Controller<OverlayView, OverlayViewModel> {
        private readonly AppData _appData;
        private readonly LoggingService _logger;
        private readonly Configuration _configuration;
        private readonly IEventBus _eventBus;
        private readonly ICardZoneManager _topCardZoneManager;
        private readonly ICardZoneManager _bottomCardZoneManager;
        private readonly IList<ICardZoneManager> _cardZoneManagers = new List<ICardZoneManager>();
        private readonly DispatcherTimer _autoSnapshotTimer = new DispatcherTimer();

        public OverlayController(AppData appData, LoggingService loggingService, IEventBus eventBus) {
            loggingService.LogMessage("Initializing Overlay Controller.");
            _appData = appData;
            _logger = loggingService;
            _configuration = appData.Configuration;
            _eventBus = eventBus;
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
            eventBus.SubscribeToToggleCardInfoVisibilityRequest(ToggleCardInfoVisibilityRequestHandler);
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
                _autoSnapshotTimer.Stop();
                ClearAllCards();

                eventBus.UnsubscribeFromButtonInfoChanged(ButtonInfoChangedHandler);
                eventBus.UnsubscribeFromButtonRemoved(ButtonRemovedHandler);
                eventBus.UnsubscribeFromShowDeckListRequest(ShowDeckListRequestHandler);
                eventBus.UnsubscribeFromTakeSnapshotRequest(TakeSnapshotHandler);
                eventBus.UnsubscribeFromToggleCardInfoVisibilityRequest(ToggleCardInfoVisibilityRequestHandler);
                eventBus.UnsubscribeFromClearAllCardsRequest(ClearAllCardsRequestHandler);
                eventBus.UnsubscribeFromClearAllCardsForCardGroupRequest(ClearAllCardsForCardGroupRequestHandler);
                eventBus.UnsubscribeFromToggleCardZoneVisibilityRequest(ToggleCardZoneVisibilityRequestHandler);

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

        private ICardZoneManager CreateCardZoneManager(IList<OverlayCardViewModel> overlayCards) {
            var cardZoneManager = ServiceLocator.GetService<CardZoneManager>();
            cardZoneManager.SetOverlayCards(overlayCards);

            _cardZoneManagers.Add(cardZoneManager);

            return cardZoneManager;
        }

        private void ClearAllCards() {
            _topCardZoneManager.Clear();
            _bottomCardZoneManager.Clear();

            var allVisibleCardInfos = ViewModel.EncounterCardInfos.Union(ViewModel.PlayerCardInfos).ToList();
            foreach (var overlayCard in allVisibleCardInfos) {
                HideCardInfo(overlayCard.CardInfo);
            }
        }

        #region Size Calculations

        private void CalculateDeckListHeight() {
            ViewModel.DeckListHeight = _appData.Configuration.OverlayHeight - 40;
        }

        private void CalculateOverlayFontSize() {
            ViewModel.OverlayFontSize = _appData.Configuration.OverlayHeight / 24;
            ViewModel.StatFontSize = ViewModel.OverlayFontSize * .75;
        }

        private void CalculateStatImageSize() {
            ViewModel.StatImageSize = _appData.Configuration.OverlayHeight / 18;
            ViewModel.InvestigatorImageSize = ViewModel.StatImageSize * 2;
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
            var encounterCardInfosHeight = Math.Min(_configuration.CardHeight, CalculateMaxHeightForRow(ViewModel.EncounterCardInfos));
            var playerCardInfosHeight = Math.Min(_configuration.CardHeight, CalculateMaxHeightForRow(ViewModel.PlayerCardInfos));
            var bottomZoneCardsHeight = Math.Min(_configuration.BottomCardZoneHeight, CalculateMaxHeightForRow(ViewModel.BottomZoneCards));

            var isTopZoneVisible = topZoneCardsHeight > 0;
            var isEncounterCardInfosVisible = encounterCardInfosHeight > 0;
            var isPlayerCardInfosVisible = playerCardInfosHeight > 0;
            var isBottomZoneVisible = bottomZoneCardsHeight > 0;

            var margins = 50; // top and bottom
            if (isTopZoneVisible) margins += 10;
            if (isEncounterCardInfosVisible) margins += 10;
            if (isPlayerCardInfosVisible) margins += 10;
            if (isBottomZoneVisible) margins += 10;

            var overlayHeightWithMargins = _configuration.OverlayHeight - margins;

            //is there enough room to show everything?
            if ((topZoneCardsHeight + bottomZoneCardsHeight + encounterCardInfosHeight + playerCardInfosHeight) > overlayHeightWithMargins) {
                //there is not- keep the act/agenda fixed and fill the remaining space
                var spaceToFill = overlayHeightWithMargins - topZoneCardsHeight;

                //how many zones do we have to account for?
                var remainingZoneCount = 0;
                if (isEncounterCardInfosVisible) remainingZoneCount++;
                if (isPlayerCardInfosVisible) remainingZoneCount++;
                if (isBottomZoneVisible) remainingZoneCount++;

                //what is the average remaining zone height?
                var averageHeight = spaceToFill / remainingZoneCount;

                //if the encounter height is smaller than the average height, we have extra we can add to the other zones
                var encounterHeightLeftover = (!isEncounterCardInfosVisible || encounterCardInfosHeight > averageHeight) ? 0 : averageHeight - encounterCardInfosHeight;

                //if the player height is smaller than the average height, we have extra we can add to the other zones
                var playerHeightLeftover = (!isPlayerCardInfosVisible || playerCardInfosHeight > averageHeight) ? 0 : averageHeight - playerCardInfosHeight;

                //if the hand height is smaller than the average heigh, we have extra we can add to the encounter player
                var handHeightLeftover = (!isBottomZoneVisible || bottomZoneCardsHeight > averageHeight) ? 0 : averageHeight - bottomZoneCardsHeight;
                
                //hand heightLeftover needs to be split if both the encounter and player zones are visible
                if (isEncounterCardInfosVisible && isPlayerCardInfosVisible) {
                    handHeightLeftover = handHeightLeftover / 2;
                }

                encounterCardInfosHeight = Math.Min(encounterCardInfosHeight, averageHeight + handHeightLeftover);
                playerCardInfosHeight = Math.Min(playerCardInfosHeight, averageHeight + handHeightLeftover);
                bottomZoneCardsHeight = Math.Min(bottomZoneCardsHeight, averageHeight + encounterHeightLeftover + playerHeightLeftover);
            }

            SetMaxHeightForRow(ViewModel.TopZoneCards, topZoneCardsHeight / OverlayCardViewModel.CardWidthRatio);
            SetMaxHeightForRow(ViewModel.EncounterCardInfos, encounterCardInfosHeight);
            SetMaxHeightForRow(ViewModel.PlayerCardInfos, playerCardInfosHeight);
            SetMaxHeightForRow(ViewModel.BottomZoneCards, bottomZoneCardsHeight);
        }

        private double CalculateMaxHeightForRow(IList<OverlayCardViewModel> overlayCards) {
            if (!overlayCards.Any()) {
                return 0;
            }

            var margin = overlayCards.First().Margin * 2;
            var horizontalCount = overlayCards.Count(x => x.CardInfo.IsHorizontal);
            var verticalCount = overlayCards.Count(x => !x.CardInfo.IsHorizontal);
            var maxCardWidth = (_configuration.OverlayWidth - 35 - (margin * (horizontalCount + verticalCount) )) / (verticalCount + horizontalCount / OverlayCardViewModel.CardWidthRatio);

            return maxCardWidth / OverlayCardViewModel.CardWidthRatio;
        }

        private void SetMaxHeightForRow(IList<OverlayCardViewModel> overlayCards, double maxHeight) {
            foreach (var overlayCard in overlayCards) {
                overlayCard.MaxHeight = maxHeight;
            }
        }

        #endregion

        #region Event Handlers

        private void ButtonInfoChangedHandler(ButtonInfoChanged e) {
            //todo: index 0 = the "show" button- find a better way to do this
            if (e.ButtonMode == ButtonMode.Zone && e.Index > 0) {
                UpdateCardZone(GetCardZoneForCardGroup(e.CardGroupId, e.ZoneIndex));
            }
        }

        private void ButtonRemovedHandler(ButtonRemoved e) {
            //todo: index 0 = the "show" button- find a better way to do this
            if (e.ButtonMode == ButtonMode.Zone && e.Index > 0) {
                UpdateCardZone(GetCardZoneForCardGroup(e.CardGroupId, e.ZoneIndex));
            }
        }

        private void ToggleCardZoneVisibilityRequestHandler(ToggleCardZoneVisibilityRequest request) {
            ClearDeckList();

            ToggleCardZone(request.CardZone);
        }

        private void ClearAllCardsRequestHandler(ClearAllCardsRequest request) {
            ClearAllCards();
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

        private void ShowDeckListRequestHandler(ShowDeckListRequest request) {
            ShowDeckList(GetPlayerFromId(request.CardGroupId));
        }

        internal void ToggleCardInfoVisibilityRequestHandler(ToggleCardInfoVisibilityRequest request) {
            var cardInfo = request.CardInfo;

            _logger.LogMessage($"Showing card {cardInfo.Name} in overlay.");
            ClearDeckList();

            ToggleCardInfoVisibility(cardInfo);
        }

        private void ClearAllCardsForCardGroupRequestHandler(ClearAllCardsForCardGroupRequest request) {
            var cardGroup = request.CardGroup;
            foreach (var cardZone in cardGroup.CardZones) {
                if (IsCardZoneVisible(cardZone)) {
                    ToggleCardZone(cardZone);
                }
            }

            foreach (var cardInfo in cardGroup.CardPool) {
                if (IsCardInfoVisible(cardInfo)) {
                    ToggleCardInfoVisibility(cardInfo);
                }
            }
        }
        #endregion

        #region ShowDeckList
        private Player _currentDisplayedDeckList = null;
        private void ShowDeckList(Player player) {
            if (player == default) {
                return;
            }

            _logger.LogMessage("Showing deck list in overlay.");

            //it's already displayed- just hide it
            if (_currentDisplayedDeckList == player) {
                ClearDeckList();
                return;
            }

            ViewModel.DeckList = player.GetDeckList();

            _currentDisplayedDeckList = player;

            ViewModel.ShowDeckList = true;
        }

        private void ClearDeckList() {
            _logger.LogMessage("Clearing deck list in overlay.");
            ViewModel.ShowDeckList = false;
            ViewModel.DeckList = null;
            _currentDisplayedDeckList = null;
        }
        #endregion

        #region Card Info
        private void ToggleCardInfoVisibility(CardInfo cardInfo) {
            if (IsCardInfoVisible(cardInfo)) {
                HideCardInfo(cardInfo);
            } else {
                ShowCardInfo(cardInfo);
            }
        }

        private void HideCardInfo(CardInfo cardInfo) {
            var overlayCards = GetOverlayCardInfoCardList(cardInfo);
            var overlayCard = overlayCards.FirstOrDefault(x => x.CardInfo == cardInfo);
            if (overlayCard != null) {
                overlayCards.RemoveOverlayCards(overlayCard);
            }

            _eventBus.PublishCardInfoVisibilityChanged(overlayCard.CardInfo.ImageId, false);
        }

        private void ShowCardInfo(CardInfo cardInfo) {
            var overlayCards = GetOverlayCardInfoCardList(cardInfo);

            var newOverlayCard = new OverlayCardViewModel(ViewModel.AppData.Configuration, OverlayCardType.Info) { CardInfo = cardInfo };

            var overlayCardToReplace = overlayCards.FindCardInfoToReplace(cardInfo);
            if (overlayCardToReplace == null) {
                overlayCards.AddOverlayCard(newOverlayCard);
            } else {
                _eventBus.PublishCardInfoVisibilityChanged(overlayCardToReplace.CardInfo.ImageId, false);
                overlayCards[overlayCards.IndexOf(overlayCardToReplace)] = newOverlayCard;
            }

            _eventBus.PublishCardInfoVisibilityChanged(cardInfo.ImageId, true); 
        }
        #endregion

        #region Card Zones
        private void ToggleCardZone(CardZone cardZone) {
            var cardZoneManager = cardZone.Location == CardZoneLocation.Top ? _topCardZoneManager : _bottomCardZoneManager;
            cardZoneManager.ToggleVisibility(cardZone);
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
        #endregion

        #region Snapshots
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
        #endregion

        #region Utils
        private bool IsCardZoneVisible(CardZone cardZone) {
            return _cardZoneManagers.Any(x => x.IsShowingCardZone(cardZone));
        }

        private bool IsCardInfoVisible(CardInfo cardInfo) {
            return GetAllOverlayCardsForCardInfos().Any(x => x.CardInfo == cardInfo);
        }

        private IEnumerable<OverlayCardViewModel> GetAllOverlayCardsForCardInfos() {
            return ViewModel.EncounterCardInfos.Union(ViewModel.PlayerCardInfos);
        }

        private CardZone GetCardZoneForCardGroup(CardGroupId cardGroupId, int zoneIndex) {
            foreach (var cardGroup in _appData.Game.AllCardGroups) {
                if (cardGroup.Id == cardGroupId) {
                    return cardGroup.GetCardZone(zoneIndex);
                }
            }
            return null;
        }

        private ObservableCollection<OverlayCardViewModel> GetOverlayCardInfoCardList(CardInfo card) {
            if (card.IsPlayerCard) {
                return ViewModel.PlayerCardInfos;
            }

            return ViewModel.EncounterCardInfos;
        }

        private Player GetPlayerFromId(CardGroupId id) {
            switch (id) {
                case CardGroupId.Player1:
                    return _appData.Game.Players[0];
                case CardGroupId.Player2:
                    return _appData.Game.Players[1];
                case CardGroupId.Player3:
                    return _appData.Game.Players[2];
                case CardGroupId.Player4:
                    return _appData.Game.Players[3];
                default:
                    return null;
            }
        }
        #endregion
    }
}
