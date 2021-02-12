using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Data;
using ArkhamOverlay.Services;
using PageController;
using System;
using System.Windows.Controls;

namespace ArkhamOverlay.Pages.SelectCards {
    public class SelectCardsController : Controller<SelectCardsView, SelectCardsViewModel> {
        private readonly LoggingService _logger;
        public SelectCardsController(LoggingService loggingService) {
            _logger = loggingService;
            View.Closed += (s, e) => {
                Closed?.Invoke();
            };
        }

        public AppData AppData { get; set; }

        public event Action Closed;

        public ISelectableCards SelectableCards { get => ViewModel.SelectableCards; set => ViewModel.SelectableCards = value; }

        internal void Close() {
            _logger.LogMessage($"Closing Select Cards Window {ViewModel.SelectableCards.Name}");
            View.Close();
        }

        internal void Activate() {
            _logger.LogMessage($"Activating Select Cards Window {ViewModel.SelectableCards.Name}");
            View.Activate();
        }

        internal void Show() {
            _logger.LogMessage($"Showing Select Cards Window {ViewModel.SelectableCards.Name}");
            View.Show();
        }

        public double Top { get => View.Top; set => View.Top = value; }
        public double Left { get => View.Left; set => View.Left = value; }
        public double Width { get => View.Width; set => View.Width = value; }
        public double Height { get => View.Height; set => View.Height = value; }

        [Command]
        public void CardLeftClick(IButton card) {
            _logger.LogMessage($"Left clicking button {card.Text}");
            card.LeftClick();
        }

        [Command]
        public void CardRightClick(IButton card) {
            _logger.LogMessage($"Right clicking button {card.Text}");
            if (card is CardTemplateButton showCardButton) {
                if (showCardButton.CardTemplate.Type == CardType.Enemy || showCardButton.CardTemplate.Type == CardType.Treachery) {
                    var contextMenu = View.FindResource("cmSelectPlayer") as ContextMenu;
                    contextMenu.DataContext = new SelectPlayerMenuViewModel(AppData.Game, showCardButton);
                    contextMenu.IsOpen = true;
                    return;
                }
            }

            card.RightClick();
        }
    }
}
