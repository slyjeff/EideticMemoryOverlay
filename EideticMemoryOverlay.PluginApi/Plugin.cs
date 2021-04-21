namespace EideticMemoryOverlay.PluginApi {
    public interface IPlugin {
        string Name { get; }
    }
    
    public abstract class Plugin : IPlugin {
        protected Plugin(string name) {
            Name = name;
        }

        public abstract void SetUp();

        public string Name { get; private set; }
    }
}
