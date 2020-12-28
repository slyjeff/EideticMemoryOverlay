using System.Windows;
using System.Windows.Controls;

namespace ArkhamOverlay {
    public partial class PlayerCards : Window {
        public PlayerCards() {
            InitializeComponent();
        }

        public Player Player {
            set {
                DataContext = value;
            }

            get {
                return DataContext as Player;
            }
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
    }
}
