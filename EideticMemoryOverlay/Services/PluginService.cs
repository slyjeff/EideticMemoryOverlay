using EideticMemoryOverlay.PluginApi;
using EideticMemoryOverlay.PluginApi.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Emo.Services {
    public interface IPlugInService {
        /// <summary>
        /// Search available plugins for a specific plugin and return it
        /// </summary>
        /// <param name="pluginName">assembbly name of the plugin</param>
        /// <returns>plugin with game specific logic for the overlay</returns>
        PlugIn GetPlugInByName(string pluginName);

        /// <summary>
        /// Load all available plugins
        /// </summary>
        /// <returns>list of plugins with game specific logic for the overlay</returns>
        IList<PlugIn> LoadPlugIns(); 
    }

    public class PlugInService : IPlugInService {
        private readonly IDictionary<string, PlugIn> _plugInCache = new Dictionary<string, PlugIn>();

        private readonly ILoggingService _logger;
        private readonly IContainer _container;
        private readonly string _pluginDirectory;

        public PlugInService(ILoggingService logger, IContainer container) {
            _logger = logger;
            _container = container;
            _pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Search available plugins for a specific plugin and return it
        /// </summary>
        /// <param name="pluginName">Name of the plugin</param>
        /// <returns>plugin with game specific logic for the overlayy</returns>
        public PlugIn GetPlugInByName(string pluginName) {
            try {
                var pluginPath = _pluginDirectory + "\\" + (string.IsNullOrEmpty(pluginName) ? "EmoPlugIn.ArkhamHorrorLcg.dll" : pluginName);
                return LoadPluginAssembly(pluginPath);
            } catch (Exception e) {
                _logger.LogException(e, $"Error loading plugin assembly {pluginName}");
                return default;
            }
        }

        /// <summary>
        /// Load all available plugins
        /// </summary>
        /// <returns>list of plugins with game specific logic for the overlay</returns>
        public IList<PlugIn> LoadPlugIns() {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pluginAssemblies = Directory.GetFiles(directory, "EmoPlugin*.dll");
            var plugins = new List<PlugIn>();
            foreach (var assemblyName in pluginAssemblies) {
                try {
                    var plugin = LoadPluginAssembly(assemblyName);
                    if (plugin != default) {
                        plugins.Add(plugin);
                    }
                } catch (Exception e) {
                    _logger.LogException(e, $"Error loading plugin assembly {assemblyName}");
                }
            }

            return plugins;
        }

        private PlugIn LoadPluginAssembly(string filename) {
            if (_plugInCache.ContainsKey(filename)) {
                _logger.LogMessage($"loading plugin {filename} from cache");
                return _plugInCache[filename];
            }

            var assembly = Assembly.LoadFile(filename);
            if (assembly == default) {
                _logger.LogError($"Error loading plugin assembly {filename}");
                return default;
            }

            var pluginType = assembly.GetTypes().Where(x => typeof(PlugIn).IsAssignableFrom(x)).FirstOrDefault();
            if (pluginType == default) {
                _logger.LogError($"Error loading plugin assembly {filename}");
                return default;
            }

            var plugIn = (PlugIn)Activator.CreateInstance(pluginType);
            plugIn.SetUp(_container);

            _plugInCache[filename] = plugIn;

            return plugIn;
        }
    }
}
