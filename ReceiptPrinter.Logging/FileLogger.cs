using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ReceiptPrinter.Logging.Interfaces;
using ReceiptPrinter.Logging.Models;

namespace ReceiptPrinter.Logging
{
    public class FileLogger : ILogger, IDisposable
    {
        private readonly string _primaryLogDirectory;
        private readonly string _outputLogDirectory;
        private readonly int _retentionDays;
        private readonly LogLevel _minimumLevel;
        private readonly object _lock = new object();
        private bool _disposed = false;

        /* ----------------------------------------------------------
         *  NEW CONSTRUCTOR – receives everything it needs from LogManager
         * ---------------------------------------------------------- */
        public FileLogger(string primaryLogDirectory,
                          string outputLogDirectory,
                          int retentionDays,
                          LogLevel minimumLevel)
        {
            _primaryLogDirectory = primaryLogDirectory ?? throw new ArgumentNullException(nameof(primaryLogDirectory));
            _outputLogDirectory = outputLogDirectory ?? throw new ArgumentNullException(nameof(outputLogDirectory));
            _retentionDays = retentionDays;
            _minimumLevel = minimumLevel;
            EnsureDirectories();
            CleanupPrimaryLogs();
        }

        /* ----------------------------------------------------------
         *  Parameter-less ctor for legacy code – uses the defaults that
         *  are now exposed as public const fields on LogManager.
         * ---------------------------------------------------------- */
        public FileLogger()
            : this(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogManager.DefaultLogDirectory),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output", "Logs"),
                LogManager.DefaultRetentionDays,
                LogManager.DefaultMinimumLevel)
        {
        }

        private void EnsureDirectories()
        {
            try
            {
                if (!Directory.Exists(_primaryLogDirectory))
                    Directory.CreateDirectory(_primaryLogDirectory);
                if (!Directory.Exists(_outputLogDirectory))
                    Directory.CreateDirectory(_outputLogDirectory);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create log directories", ex);
            }
        }

        private void CleanupPrimaryLogs()
        {
            try
            {
                var cutoff = DateTime.Now.AddDays(-_retentionDays);
                var files = Directory.GetFiles(_primaryLogDirectory, "ReceiptPrinter_*.log");
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoff)
                    {
                        try
                        {
                            fileInfo.Delete();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[LOG CLEANUP FAILED] {file}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOG CLEANUP ERROR] {ex.Message}");
            }
        }

        private string GetPrimaryLogFilePath() =>
            Path.Combine(_primaryLogDirectory, $"ReceiptPrinter_{DateTime.Now:yyyyMMdd}.log");

        private string GetOutputLogFilePath() =>
            Path.Combine(_outputLogDirectory, $"ReceiptPrinter_FULL_{DateTime.Now:yyyyMMdd}.log");

        private void WriteToFile(string path, string content)
        {
            try
            {
                File.AppendAllText(path, content + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOG WRITE FAILED] {path}: {ex.Message}");
            }
        }

        private void Write(LogLevel level, string message, Exception ex = null)
        {
            if (level < _minimumLevel) return;
            var entry = new LogEntry(level, message, ex);
            var logLine = entry.ToString();
            lock (_lock)
            {
                WriteToFile(GetPrimaryLogFilePath(), logLine);
                WriteToFile(GetOutputLogFilePath(), logLine);
            }
        }

        private async Task WriteAsync(LogLevel level, string message, Exception ex = null)
        {
            if (level < _minimumLevel) return;
            var entry = new LogEntry(level, message, ex);
            var logLine = entry.ToString();
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    WriteToFile(GetPrimaryLogFilePath(), logLine);
                    WriteToFile(GetOutputLogFilePath(), logLine);
                }
            });
        }

        public void Debug(string message) => Write(LogLevel.DEBUG, message);
        public void Info(string message) => Write(LogLevel.INFO, message);
        public void Warning(string message) => Write(LogLevel.WARNING, message);
        public void Error(string message, Exception ex = null) => Write(LogLevel.ERROR, message, ex);
        public void Fatal(string message, Exception ex = null) => Write(LogLevel.FATAL, message, ex);

        public async Task DebugAsync(string message) => await WriteAsync(LogLevel.DEBUG, message);
        public async Task InfoAsync(string message) => await WriteAsync(LogLevel.INFO, message);
        public async Task WarningAsync(string message) => await WriteAsync(LogLevel.WARNING, message);
        public async Task ErrorAsync(string message, Exception ex = null) => await WriteAsync(LogLevel.ERROR, message, ex);
        public async Task FatalAsync(string message, Exception ex = null) => await WriteAsync(LogLevel.FATAL, message, ex);

        public void Dispose()
        {
            if (!_disposed)
            {
                // Perform any necessary cleanup here
                _disposed = true;
            }
        }
    }
}
