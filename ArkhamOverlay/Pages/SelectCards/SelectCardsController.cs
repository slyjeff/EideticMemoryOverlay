using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Common.Utils;
using ArkhamOverlay.Data;
using ArkhamOverlay.Events;
using ArkhamOverlay.Services;
using PageController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace ArkhamOverlay.Pages.SelectCards {
    public class SelectCardsController : Controller<SelectCardsView, SelectCardsViewModel>, IButtonOptionResolver {
        private readonly LoggingService _logger;
        private readonly AppData _appData;
        private readonly IEventBus _eventBus;

        public SelectCardsController(LoggingService loggingService, AppData appData, IEventBus eventBus) {
            _logger = loggingService;
            _appData = appData;
            _eventBus = eventBus;
            View.Closed += (s, e) => {
                Closed?.Invoke();
            };
        }

        public event Action Closed;

        public ICardGroup CardGroup { get => ViewModel.CardGroup; set => ViewModel.CardGroup = value; }

        internal void Close() {
            _logger.LogMessage($"Closing Select Cards Window {ViewModel.CardGroup.Name}");
            View.Close();
        }

        internal void Activate() {
            _logger.LogMessage($"Activating Select Cards Window {ViewModel.CardGroup.Name}");
            View.Activate();
        }

        internal void Show() {
            _logger.LogMessage($"Showing Select Cards Window {ViewModel.CardGroup.Name}");
            View.Show();
        }

        public double Top { get => View.Top; set => View.Top = value; }
        public double Left { get => View.Left; set => View.Left = value; }
        public double Width { get => View.Width; set => View.Width = value; }
        public double Height { get => View.Height; set => View.Height = value; }

        [Command]
        public void CardInfoLeftClick(IButton button) {
            _logger.LogMessage($"Left clicking button {button.Text}");

            var index = ViewModel.CardGroup.CardButtons.IndexOf(button);
            _eventBus.PublishButtonClickRequest(ViewModel.CardGroup.Id, ButtonMode.Pool, 0, index, MouseButton.Left);
        }

        [Command]
        public void CardInfoRightClick(IButton button) {
            _logger.LogMessage($"Right clicking button {button.Text}");

            var index = ViewModel.CardGroup.CardButtons.IndexOf(button);
            RightClick(ButtonMode.Pool, 0, index, button);
        }

        [Command]
        public void CardLeftClick(CardButton button) {
            _logger.LogMessage($"Left clicking button {button.Text}");

            var buttonLocation = FindButtonLocation(button);
            if (buttonLocation == default) {
                return;
            }

            _eventBus.PublishButtonClickRequest(ViewModel.CardGroup.Id, ButtonMode.Zone, buttonLocation.ZoneIndex, buttonLocation.Index, MouseButton.Left);
        }

        [Command]
        public void CardRightClick(CardButton button) {
            _logger.LogMessage($"Right clicking button {button.Text}");
            
            var buttonLocation = FindButtonLocation(button);
            if (buttonLocation == default) {
                return;
            }

            RightClick(ButtonMode.Zone, buttonLocation.ZoneIndex, buttonLocation.Index, button);
        }

        /// <summary>
        /// Represents the location of a button in a card group by zone and index
        /// </summary>
        private class ButtonLocation {
            /// <summary>
            /// Index of the zone within the card group
            /// </summary>
            public int ZoneIndex;

            /// <summary>
            /// Index of the button within the zone
            /// </summary>
            public int Index;
        }
        
        /// <summary>
        /// Find the location of a button within the card group
        /// </summary>
        /// <param name="button">The button</param>
        /// <returns></returns>
        private ButtonLocation FindButtonLocation(CardButton button) {
            foreach (var zone in ViewModel.CardGroup.CardZones) {
                var index = zone.Buttons.IndexOf(button);
                if (index == -1) {
                    continue;
                }

                return new ButtonLocation {
                    ZoneIndex = ViewModel.CardGroup.CardZones.IndexOf(zone),
                    Index = index
                };
            }
            return default;
        }

        /// <summary>
        /// Handle right click, popping a menu if the button requires it
        /// </summary>
        /// <param name="buttonMode">Whether this is a pool or zone button</param>
        /// <param name="zoneIndex">Which zone in the button is in</param>
        /// <param name="index">Location of the button within the zone/pool</param>
        /// <param name="button">The button clicked</param>
        private void RightClick(ButtonMode buttonMode, int zoneIndex, int index, IButton button) {
            try {
                if (!button.Options.Any()) {
                    return;
                }

                if (button.Options.Count == 1) {
                    _eventBus.PublishButtonClickRequest(ViewModel.CardGroup.Id, buttonMode, zoneIndex, index, MouseButton.Right, button.Options.First());
                    return;
                }

                var contextMenu = View.FindResource("cmSelectPlayer") as ContextMenu;
                contextMenu.ItemsSource = CreateRightClickOptions(button.Options, selectedOption => _eventBus.PublishButtonClickRequest(ViewModel.CardGroup.Id, buttonMode, zoneIndex, index, MouseButton.Right, selectedOption));
                contextMenu.IsOpen = true;
            } catch (Exception e) {
                _logger.LogException(e, "Error Handling Right Click");
            }
        }

        /// <summary>
        /// Generate commands that can be assigned to the 
        /// </summary>
        /// <param name="options">The options the user may selectd from when clicking this button</param>
        /// <param name="callback">What to do when the user selects an option</param>
        /// <returns>A list of menu items</returns>
        private IEnumerable<RightClickOptionCommand> CreateRightClickOptions(IEnumerable<ButtonOption> options, Action<ButtonOption> callback) {
            var commands = new List<RightClickOptionCommand>();
            foreach (var option in options) {
                var text = option.GetText(this);
                if (!string.IsNullOrEmpty(text)) {
                    commands.Add(new RightClickOptionCommand(option, text, callback));
                }
            }

            return commands;
        }

        /// <summary>
        /// Used by button option to get a name for the card group when displaying an option
        /// </summary>
        /// <param name="cardGroupId">Card group to resolve</param>
        /// <returns>The name of the card group</returns>
        string IButtonOptionResolver.GetCardGroupName(CardGroupId cardGroupId) {
            var cardGroup = _appData.Game.GetCardGroup(cardGroupId);
            return cardGroup.Name;
        }

        /// <summary>
        /// Used by button option to get a name for the card zone when displaying an option
        /// </summary>
        /// <param name="cardGroupId">Card group of the card zone</param>
        /// <param name="zoneIndex">Zone to resolve</param>
        /// <returns>Name of the zone</returns>
        string IButtonOptionResolver.GetCardZoneName(CardGroupId cardGroupId, int zoneIndex) {
            var cardGroup = _appData.Game.GetCardGroup(cardGroupId);
            var cardZone = cardGroup.GetCardZone(zoneIndex);
            if (cardZone == default) {
                return string.Empty;
            }
            return cardZone.Name;
        }

        /// <summary>
        /// Used by button option to get the image ID for a button when displaying an option
        /// </summary>
        /// <param name="cardGroupId">Card group of the card zone</param>
        /// <param name="zoneIndex">Zone to resolve</param>
        /// <returns>Image Id for the card group</returns>
        string IButtonOptionResolver.GetImageId(CardGroupId cardGroupId, int zoneIndex) {
            var cardGroup = _appData.Game.GetCardGroup(cardGroupId);
            return cardGroup.Name;
        }

    }
}
