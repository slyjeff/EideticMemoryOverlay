using System;

namespace PageController {
    public static class PageControllerConfiguration {
        public static IPageDependencyResolver PageDependencyResolver = new DefaultPageDependencyResolver(); 
        public static bool ResolveViews = true;
    }

    class DefaultPageDependencyResolver : IPageDependencyResolver {
        public object Resolve(Type type) {
            return Activator.CreateInstance(type);
        }
    } 
}
