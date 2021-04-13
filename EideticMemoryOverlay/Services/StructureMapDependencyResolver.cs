using System;
using StructureMap;

namespace Emo.Services {
    public class StructureMapDependencyResolver : PageController.IPageDependencyResolver {
        private readonly Container _container; 
        public StructureMapDependencyResolver(Container container) {
            _container = container;
        }

        public object Resolve(Type type) {
            return _container.GetInstance(type);
        }
    }
}
