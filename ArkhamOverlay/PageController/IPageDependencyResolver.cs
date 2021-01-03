using System;

namespace PageController {
    static class PageDependencyResolverExtensions {
        public static T Resolve<T>(this IPageDependencyResolver pageDependencyResolver) {
            return (T)pageDependencyResolver.Resolve(typeof(T));
        }
    }

    public interface IPageDependencyResolver {
        object Resolve(Type type);
    }
}
