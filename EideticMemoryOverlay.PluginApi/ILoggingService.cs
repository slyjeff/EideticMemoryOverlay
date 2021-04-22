using System;

namespace EideticMemoryOverlay.PluginApi {
    public interface ILoggingService {
        void LogMessage(string message);

        void LogWarning(string message);

        void LogError(string message);

        void LogException(Exception ex, string message);
    }
}
