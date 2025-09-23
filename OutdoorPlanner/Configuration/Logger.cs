using NLog;
using NLog.Config;
using NLog.Targets;

namespace OutdoorPlanner.Configuration
{
    public static class Logger
    {
        private static readonly Lazy<NLog.Logger> _lazyLogger = new Lazy<NLog.Logger>(ConfigureLogger);

        public static NLog.Logger Log => _lazyLogger.Value;

        private static NLog.Logger ConfigureLogger()
        {
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget("filedata")
            {
                FileName = Path.Combine("${basedir}", "logs", "${shortdate}.log"),
                Layout = "${longdate} ${uppercase:${level}} ${message}"
            };

            config.AddTarget(fileTarget);

            var rule = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;

            return LogManager.GetCurrentClassLogger();
        }

        public static void Debug(string message) => Log.Debug(message);
        public static void Info(string message) => Log.Info(message);
        public static void Warn(string message) => Log.Warn(message);
        public static void Error(string message) => Log.Error(message);
        public static void Error(Exception ex, string message) => Log.Error(ex, message);
        public static void Fatal(string message) => Log.Fatal(message);
    }
}