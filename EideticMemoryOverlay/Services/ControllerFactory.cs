using EideticMemoryOverlay.PluginApi;
using PageController;
using StructureMap;
using System;

namespace Emo.Services {
    public interface IDisplayableView {
        void ShowView();
    } 

    public interface IControllerFactory {
        /// <summary>
        /// Create a controller
        /// </summary>
        /// <typeparam name="T">Type of the controller to create</typeparam>
        /// <returns>Instance of the controller</returns>
        T CreateController<T>() where T : Controller;

        /// <summary>
        /// Create a controller that accepts a local cards type as a genric parameter- this will be supplied by the current plugin
        /// </summary>
        /// <param name="type">Type of the controller to create</param>
        /// <returns>Instance of the controller</returns>
        IDisplayableView CreateLocalCardsController(Type type);
    }

    internal class ControllerFactory : IControllerFactory {
        private readonly Container _container;
        public ControllerFactory(Container container) {
            _container = container;
        }

        public T CreateController<T>() where T : Controller {
            return _container.GetInstance<T>();
        }

        /// <summary>
        /// Create a controller that accepts a local cards type as a genric parameter- this will be supplied by the current plugin
        /// </summary>
        /// <param name="type">Type of the controller to create</param>
        /// <returns>Instance of the controller</returns>
        public IDisplayableView CreateLocalCardsController(Type type) {
            var plugIn = _container.GetInstance<IPlugIn>();
            if (plugIn == default) {
                return default;
            }

            var genericType = type.MakeGenericType(plugIn.LocalCardType);
            return _container.GetInstance(genericType) as IDisplayableView;
        }
    }
}
