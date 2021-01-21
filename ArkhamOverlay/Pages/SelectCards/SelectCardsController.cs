using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Data;
using PageController;
using System;
using System.Windows.Controls;

namespace ArkhamOverlay.Pages.SelectCards {
    public class SelectCardsController : Controller<SelectCardsView, SelectCardsViewModel> {
        public SelectCardsController() {
            View.Closed += (s, e) => {
                Closed?.Invoke();
            };
        }

        public AppData AppData { get; set; }

        public event Action Closed;

        public ISelectableCards SelectableCards { get => ViewModel.SelectableCards; set => ViewModel.SelectableCards = value; }

        internal void Close() {
            View.Close();
        }

        internal void Activate() {
            View.Activate();
        }

        internal void Show() {
            View.Show();
        }

        public double Top { get => View.Top; set => View.Top = value; }
        public double Left { get => View.Left; set => View.Left = value; }
        public double Width { get => View.Width; set => View.Width = value; }
        public double Height { get => View.Height; set => View.Height = value; }

        [Command]
        public void CardLeftClick(ICardButton card) {
            card.LeftClick();
        }

        [Command]
        public void CardRightClick(ICardButton card) {
            if (card is ShowCardButton showCardButton) {
                if (showCardButton.Card.Type == CardType.Enemy || showCardButton.Card.Type == CardType.Treachery) {
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
