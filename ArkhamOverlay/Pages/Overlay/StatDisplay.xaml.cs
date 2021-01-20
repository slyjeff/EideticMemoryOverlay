using System.Windows;
using System.Windows.Controls;

namespace ArkhamOverlay.Pages.Overlay {
    public partial class StatDisplay : Border {
        public StatDisplay() {
            InitializeComponent();
        }

        public HorizontalAlignment ImageAlignment { get => Images.HorizontalAlignment; set => Images.HorizontalAlignment = value; }
    }
}
