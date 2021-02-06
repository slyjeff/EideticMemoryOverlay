using StructureMap;

namespace ArkhamOverlay.Utils {
    // TODO: Consolidate in common project
    public static class ServiceLocator {
        public static Container Container { get; set; }

        public static T GetService<T>() {
            return Container.GetInstance<T>();
        }
    }
}