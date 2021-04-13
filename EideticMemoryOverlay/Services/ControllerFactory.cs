using PageController;
using StructureMap;

namespace Emo.Services {
    public interface IControllerFactory {
        T CreateController<T>() where T : Controller;
    }

    public class ControllerFactory : IControllerFactory {
        private readonly Container _container;
        public ControllerFactory(Container container) {
            _container = container;
        }

        public T CreateController<T>() where T : Controller {
            return _container.GetInstance<T>();
        }
    }
}
