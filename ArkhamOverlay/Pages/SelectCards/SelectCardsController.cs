using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Common.Enums;
using ArkhamOverlay.Common.Services;
using ArkhamOverlay.Data;
using ArkhamOverlay.Events;
using ArkhamOverlay.Services;
using ArkhamOverlay.Utils;
using PageController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace ArkhamOverlay.Pages.SelectCards {
    public class SelectCardsController : Controller<SelectCardsView, SelectCardsViewModel> {
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
        public void CardTemplateLeftClick(IButton button) {
            _logger.LogMessage($"Left clicking button {button.Text}");

            var index = ViewModel.CardGroup.CardButtons.IndexOf(button);
            _eventBus.PublishButtonClickRequest(ViewModel.CardGroup.Id, ButtonMode.Pool, index, MouseButton.Left, string.Empty);
        }

        [Command]
        public void CardTemplateRightClick(IButton button) {
            _logger.LogMessage($"Right clicking button {button.Text}");

            var index = ViewModel.CardGroup.CardButtons.IndexOf(button);
            RightClick(ButtonMode.Pool, index, button);
        }

        [Command]
        public void CardLeftClick(CardButton button) {
            _logger.LogMessage($"Left clicking button {button.Text}");
            var index = ViewModel.CardGroup.CardZone.Buttons.IndexOf(button);
            _eventBus.PublishButtonClickRequest(ViewModel.CardGroup.Id, ButtonMode.Zone, index, MouseButton.Left, string.Empty);
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
                if (button is CardImageButton cardImageButton) {
                    if (cardImageButton.Options.Any()) {
                        var contextMenu = View.FindResource("cmSelectPlayer") as ContextMenu;
                        contextMenu.ItemsSource = CreateRightClickOptions(cardImageButton.Options, selectedOption => _eventBus.PublishButtonClickRequest(ViewModel.CardGroup.Id, ButtonMode.Pool, index, MouseButton.Right, selectedOption));
                        contextMenu.IsOpen = true;
                        return;
                    }
                }

                _eventBus.PublishButtonClickRequest(ViewModel.CardGroup.Id, buttonMode, index, MouseButton.Right, string.Empty);
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
        private IEnumerable<RightClickOptionCommand> CreateRightClickOptions(IEnumerable<ButtonOption> options, Action<string> callback) {
            var commands = new List<RightClickOptionCommand>();
            foreach (var option in options) {
                var text = option.GetTextResolvingPlaceholders(ResolveMenuItemPlaceholder);
                if (!string.IsNullOrEmpty(text)) {
                    commands.Add(new RightClickOptionCommand(option.Option, text, callback));
                }
            }

            return commands;
        }

        /// <summary>
        /// Callback to resolve placeholder in a menu item so we can provide more contextual information
        /// </summary>
        /// <param name="placeholder">The placeholder to resolve</param>
        /// <returns>The actual value the placeholder represents</returns>
        /// <remarks>Example "player1" (represented by <<xxxx>></xxxx> in the text) will resolve to the name of player 1 in game data</remarks>
        private string ResolveMenuItemPlaceholder(string placeholder) {
            if (Enum.TryParse(placeholder, out CardGroupId cardGroupId)) {
                var cardGroup = _appData.Game.GetCardGroup(cardGroupId);
                return cardGroup.Name;
            }

            return null;
        }
    }
}
