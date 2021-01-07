using ArkhamOverlay.CardButtons;
using ArkhamOverlay.Data;
using PageController;
using System;

namespace ArkhamOverlay.Pages.SelectCards {
    public class SelectCardsController : Controller<SelectCardsView, SelectCardsViewModel> {
        public SelectCardsController() {
            View.Closed += (s, e) => {
                Closed?.Invoke();
            };
        }

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
        public void CardSelected(ICardButton card) {
            card.LeftClick();
        }
    }
}
