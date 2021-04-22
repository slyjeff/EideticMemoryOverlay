using System.IO;

namespace EideticMemoryOverlay.PluginApi {
    public interface ILocalCard {
        string FilePath { get; set; }
        string Name { get; set; }
        bool HasBack { get; set; }
    }

    public abstract class LocalCard : ILocalCard {
        public string FilePath { get; set; }
        public string Name { get; set; }
        public bool HasBack { get; set; }

        public string BackFilePath {
            get {
                return Path.GetDirectoryName(FilePath) + "\\" + Path.GetFileNameWithoutExtension(FilePath) + "-back" + Path.GetExtension(FilePath);
            }
        }
    }
}
