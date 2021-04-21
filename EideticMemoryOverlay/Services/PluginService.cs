using CommonServiceLocator;
using EideticMemoryOverlay.PluginApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Emo.Services {
    public interface IPluginService {
        IList<IPlugin> LoadPlugins();
    }

    public class PluginService : IPluginService {
        private readonly LoggingService _logger;

        public PluginService(LoggingService logger) {
            _logger = logger;
        }

        public IList<IPlugin> LoadPlugins() {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var gameAssemblies = Directory.GetFiles(directory, "EmoPlugin*.dll");
            var plugins = new List<IPlugin>();
            var iPluginType = typeof(IPlugin);
            foreach (var assemblyName in gameAssemblies) {
                try {
                    var assembly = Assembly.LoadFile(assemblyName);
                    if (assembly == default) {
                        continue;
                    }

                    var pluginsInAssembly = assembly.GetTypes().Where(x => iPluginType.IsAssignableFrom(x));
                    foreach (var pluginType in pluginsInAssembly) {
                        plugins.Add((IPlugin)Activator.CreateInstance(pluginType));
                    }
                } catch (Exception e) {
                    _logger.LogException(e, $"Error loading game assembly {assemblyName}");
                }

            }

            return plugins;
        }
    }
}
