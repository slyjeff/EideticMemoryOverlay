using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace EideticMemoryOverlay.PluginApi.LocalCards {
    public class EditableLocalCard : INotifyPropertyChanged, ILocalCard {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string property) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private string _filePath;
        public virtual string FilePath {
            get => _filePath;
            set {
                _filePath = value;
                if (string.IsNullOrEmpty(Name)) {
                    Name = Path.GetFileNameWithoutExtension(_filePath);
                }
            }
        }

        private string _name;
        public virtual string Name {
            get => _name;
            set {
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        public virtual bool HasBack { get; set; }
        public virtual ImageSource Image { get; set; }
        public virtual ImageSource FrontThumbnail { get; set; }
        public virtual ImageSource BackThumbnail { get; set; }
        public Rect ClipRect { get; set; }
    }

}
