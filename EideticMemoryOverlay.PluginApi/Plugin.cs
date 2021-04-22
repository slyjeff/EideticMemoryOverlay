namespace EideticMemoryOverlay.PluginApi {
    public interface IPlugIn {
        string Name { get; }
    }
    
    public abstract class PlugIn : IPlugIn {
        protected PlugIn(string name) {
            Name = name;
        }

        public abstract void SetUp();

        public string Name { get; private set; }
    }
}
