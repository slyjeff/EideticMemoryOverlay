using System;

namespace EideticMemoryOverlay.PluginApi.Interfaces {
    public interface ILoggingService {
        void LogMessage(string message);

        void LogWarning(string message);

        void LogError(string message);

        void LogException(Exception ex, string message);
    }
}
