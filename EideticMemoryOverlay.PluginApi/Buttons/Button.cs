using Emo.Common.Utils;
using Emo.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace EideticMemoryOverlay.PluginApi.Buttons {
    public interface IButton {
        string Text { get; }
        bool IsToggled { get; }
        IList<ButtonOption> Options { get; }
    }

    public abstract class Button : INotifyPropertyChanged, IButton {
        public Button() {
            Options = new List<ButtonOption>();
        }

        private string _text;
        public string Text {
            get => _text;
            set {
                _text = value;
                PropertyChanged?.Invoke(nameof(Text), null);
            }
        }

        public Brush BorderBrush { get { return IsToggled ? new SolidColorBrush(Colors.DarkGoldenrod) : new SolidColorBrush(Colors.Black); } }

        public virtual ImageSource ButtonImage { get { return ImageUtils.CreateSolidColorImage(Colors.DarkGray); } }

        private bool _isToggled;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsToggled {
            get => _isToggled;
            set {
                _isToggled = value;
                PropertyChanged?.Invoke(nameof(BorderBrush), null);
            }
        }

        public IList<ButtonOption> Options { get; }
    }
}
