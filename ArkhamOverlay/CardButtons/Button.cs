using ArkhamOverlay.Utils;
using PageController;
using System.Collections.Generic;
using System.Windows.Media;

namespace ArkhamOverlay.CardButtons {
    public interface IButton {
        string Text { get; }
        bool IsToggled { get; }
        IList<ButtonOption> Options { get; } 
    }

    public abstract class Button : ViewModel, IButton {
        public Button() {
            Options = new List<ButtonOption>();
        }

        private string _text;
        public string Text {
            get => _text;
            set {
                _text = value;
                NotifyPropertyChanged(nameof(Text));
            }
        }

        public Brush BorderBrush { get { return IsToggled ? new SolidColorBrush(Colors.DarkGoldenrod) : new SolidColorBrush(Colors.Black); } }

        public virtual ImageSource ButtonImage { get { return ImageUtils.CreateSolidColorImage(Colors.DarkGray); } }

        private bool _isToggled;
        public bool IsToggled {
            get => _isToggled;
            set {
                _isToggled = value;
                NotifyPropertyChanged(nameof(BorderBrush));
            }
        }

        public IList<ButtonOption> Options { get; }
    }
}
