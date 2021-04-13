using StructureMap;

namespace Emo.Common.Utils {
    public static class ServiceLocator {
        public static Container Container { get; set; }

        public static T GetService<T>() {
            return Container.GetInstance<T>();
        }
    }
}
