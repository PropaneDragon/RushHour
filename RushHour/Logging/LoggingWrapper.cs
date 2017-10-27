using RushHour.CimTools;
using RushHour.Experiments;

namespace RushHour.Logging
{
    internal static class LoggingWrapper
    {
        public static void Log(string log)
        {
            if (!ExperimentsToggle.WriteDebugLog)
                return;

            CimToolsHandler.CimToolBase.DetailedLogger.Log(log);
        }

        public static void LogWarning(string log)
        {
            if (!ExperimentsToggle.WriteDebugLog)
                return;

            CimToolsHandler.CimToolBase.DetailedLogger.LogWarning(log);
        }

        public static void LogError(string log)
        {
            if (!ExperimentsToggle.WriteDebugLog)
                return;

            CimToolsHandler.CimToolBase.DetailedLogger.LogError(log);
        }
    }
}
