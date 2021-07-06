using EideticMemoryOverlay.PluginApi.Interfaces;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;

namespace Emo.Services {
    public class LoggingService : ILoggingService {
        private const string _LOG_NAME = "overlayLog";
        private readonly Logger _logger;

        public LoggingService() {
            var config = new LoggingConfiguration();

            var logfileName = $"EideticMemoryOverlayLog_{DateTime.Now:yyyymmddhhmm}.txt";

            // Targets where to log to: File
            var logfile = new FileTarget("logfile") { FileName = Path.GetTempPath() + logfileName };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);

            // Apply config           
            LogManager.Configuration = config;

            _logger = LogManager.GetLogger(_LOG_NAME);
        }

        public void LogMessage(string message) => _logger.Info(message);

        public void LogWarning(string message) => _logger.Warn(message);

        public void LogError(string message) => _logger.Error(message);

        public void LogException(Exception ex, string message) => _logger.Error(ex, message);
    }
}
