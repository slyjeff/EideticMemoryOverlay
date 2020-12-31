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

        public void CardSelected(object sender, RoutedEventArgs e) {
            if (!(sender is Button button)) {
                return;
            }

            if (!(button.DataContext is ICardButton card)) {
                return;
            }

            card.Click();
        }
    }
}
