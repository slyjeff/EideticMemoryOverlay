using EideticMemoryOverlay.PluginApi;
using PageController;
using StructureMap;
using System;

namespace Emo.Services {
    internal interface IDisplayableView {
        void ShowView();
    } 

    internal interface IControllerFactory {
        /// <summary>
        /// set the plugin to use when resolving plugin sepecific logic
        /// </summary>
        /// <param name="plugIn">selected plugin for the current game</param>
        void SetPlugIn(IPlugIn plugIn);

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
        private IPlugIn _plugIn;
        private readonly Container _container;
        public ControllerFactory(Container container) {
            _container = container;
        }

        /// <summary>
        /// set the plugin to use when resolving plugin sepecific logic
        /// </summary>
        /// <param name="plugIn">selected plugin for the current game</param>
        public void SetPlugIn(IPlugIn plugIn) {
            _plugIn = plugIn;
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
            if (_plugIn == default) {
                return default;
            }

            var genericType = type.MakeGenericType(_plugIn.LocalCardType);
            return _container.GetInstance(genericType) as IDisplayableView;
        }
    }
}
