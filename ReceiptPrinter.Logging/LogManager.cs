using System;
using System.Configuration;
using System.IO;
using ReceiptPrinter.Logging.Interfaces;
using ReceiptPrinter.Logging.Models;

namespace ReceiptPrinter.Logging
{
    public sealed class LogManager
    {
        /* ----------------------------------------------------------
         *  Public constants – used by FileLogger's parameter-less ctor
         * ---------------------------------------------------------- */
        public const string DefaultLogDirectory = "Logs";
        public const int DefaultRetentionDays = 30;
        public const LogLevel DefaultMinimumLevel = LogLevel.INFO;

        private static readonly Lazy<LogManager> _instance =
            new Lazy<LogManager>(() => new LogManager());

        private readonly ILogger _logger;

        private LogManager()
        {
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Read configuration or fall back to defaults
            var logDir = ConfigurationManager.AppSettings["LogDirectory"] ?? DefaultLogDirectory;
            var retentionDays = DefaultRetentionDays;
            var minLogLevel = DefaultMinimumLevel;

            // Parse and validate retention days
            if (int.TryParse(ConfigurationManager.AppSettings["LogRetentionDays"], out int retention))
            {
                retentionDays = Math.Max(1, retention);
            }

            // Parse and validate minimum log level
            if (Enum.TryParse(ConfigurationManager.AppSettings["MinimumLogLevel"], true, out LogLevel minLevel))
            {
                minLogLevel = minLevel;
            }

            // Set properties
            LogDirectory = Path.Combine(appDirectory, logDir);
            RetentionDays = retentionDays;
            MinimumLogLevel = minLogLevel;
            OutputLogDirectory = Path.Combine(appDirectory, "Output", "Logs");

            // Create the logger with the values we just built
            _logger = new FileLogger(LogDirectory, OutputLogDirectory, RetentionDays, MinimumLogLevel);
        }

        public static LogManager Instance => _instance.Value;

        public string LogDirectory { get; }
        public string OutputLogDirectory { get; }
        public int RetentionDays { get; }
        public LogLevel MinimumLogLevel { get; }
        public ILogger Logger => _logger;

        public static ILogger GetLogger() => Instance.Logger;
    }
}
