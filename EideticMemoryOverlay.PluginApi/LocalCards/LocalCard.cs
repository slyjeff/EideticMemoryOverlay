using Newtonsoft.Json;
using System.IO;

namespace EideticMemoryOverlay.PluginApi.LocalCards {
    public interface ILocalCard {
        string FilePath { get; set; }
        string Name { get; set; }
        bool HasBack { get; set; }
    }

    public class LocalCard : ILocalCard {
        [JsonIgnore]
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public bool HasBack { get; set; }

        [JsonIgnore]
        public string BackFilePath {
            get {
                return Path.GetDirectoryName(FilePath) + "\\" + Path.GetFileNameWithoutExtension(FilePath) + "-back" + Path.GetExtension(FilePath);
            }
        }

       public virtual void CopyTo(EditableLocalCard editableLocalCard) {
            editableLocalCard.FilePath = FilePath;
            editableLocalCard.Name = Name;
            editableLocalCard.HasBack = HasBack;
        }

        public virtual void CopyFrom(EditableLocalCard editableLocalCard) {
            FileName =  Path.GetFileName(editableLocalCard.FilePath);
            Name = editableLocalCard.Name;
            HasBack = editableLocalCard.HasBack;
        }
    }
}
