using ArkhamOverlay.Data;
using System.Windows;
using System.Windows.Controls;

namespace ArkhamOverlay {
    public partial class SelectCards : Window {
        public SelectCards() {
            InitializeComponent();
        }

        public ISelectableCards SelectableCards {
            set { DataContext = value; }
            get { return DataContext as ISelectableCards; }
        }

        public Overlay Overlay { get; set; }

        public void CardSelected(object sender, RoutedEventArgs e) {
            if (!(sender is Button button)) {
                return;
            }

            if (!(button.DataContext is Card card)) {
                return;
            }

            if (Overlay == null) {
                return;
            }

            Overlay.ToggleCard(card);
        }

        public void ClearCards(object sender, RoutedEventArgs e) {
            if (Overlay == null) {
                return;
            }

            Overlay.ClearCards();
        }
    }
}
