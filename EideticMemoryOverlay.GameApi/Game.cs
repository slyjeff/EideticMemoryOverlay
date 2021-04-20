namespace EideticMemoryOverlay.GameApi {
    public abstract class Game {
        public abstract void SetUp();

        protected void SetName(string name) {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
