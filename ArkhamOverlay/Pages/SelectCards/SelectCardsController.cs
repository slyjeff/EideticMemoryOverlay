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
            _eventBus.PublishButtonClickRequest(ViewModel.CardGroup.Id, ButtonMode.Pool, index, MouseButton.Left);
        }

        [Command]
        public void CardInfoRightClick(IButton button) {
            _logger.LogMessage($"Right clicking button {button.Text}");

            var index = ViewModel.CardGroup.CardButtons.IndexOf(button);
            RightClick(ButtonMode.Pool, index, button);
        }

        [Command]
        public void CardLeftClick(CardButton button) {
            _logger.LogMessage($"Left clicking button {button.Text}");
            var index = ViewModel.CardGroup.CardZone.Buttons.IndexOf(button);
            _eventBus.PublishButtonClickRequest(ViewModel.CardGroup.Id, ButtonMode.Zone, index, MouseButton.Left);
        }

        [Command]
        public void CardRightClick(CardButton button) {
            _logger.LogMessage($"Right clicking button {button.Text}");
            var index = ViewModel.CardGroup.CardZone.Buttons.IndexOf(button);
            RightClick(ButtonMode.Zone, index, button);
        }

        /// <summary>
        /// Handle right click, popping a menu if the button requires it
        /// </summary>
        /// <param name="buttonMode">Whether this is a pool or zone button</param>
        /// <param name="index">Location of the button</param>
        /// <param name="button">The button clicked</param>
        private void RightClick(ButtonMode buttonMode, int index, IButton button) {
            try {
                if (!button.Options.Any()) {
                    return;
                }

                if (button.Options.Count == 1) {
                    _eventBus.PublishButtonClickRequest(ViewModel.CardGroup.Id, buttonMode, index, MouseButton.Right, button.Options.First());
                    return;
                }

                var contextMenu = View.FindResource("cmSelectPlayer") as ContextMenu;
                contextMenu.ItemsSource = CreateRightClickOptions(button.Options, selectedOption => _eventBus.PublishButtonClickRequest(ViewModel.CardGroup.Id, ButtonMode.Pool, index, MouseButton.Right, selectedOption));
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
            return "Hand";
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
