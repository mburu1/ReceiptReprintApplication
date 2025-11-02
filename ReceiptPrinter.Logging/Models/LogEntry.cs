using System;

namespace ReceiptPrinter.Logging.Models
{
    /// <summary>
    /// Represents a single log entry with timestamp, level, message, and optional exception.
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; } = DateTime.Now;
        public LogLevel Level { get; }
        public string Message { get; }
        public Exception Exception { get; }

        public LogEntry(LogLevel level, string message, Exception exception = null)
        {
            Level = level;
            Message = message ?? string.Empty;
            Exception = exception;
        }

        /// <summary>
        /// Returns a formatted string suitable for file output.
        /// Format: [yyyy-MM-dd HH:mm:ss.fff] [LEVEL] Message [Exception details]
        /// </summary>
        public override string ToString()
        {
            var timestamp = Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var level = Level.ToString().PadRight(7);
            var msg = Message;
            if (Exception != null)
            {
                msg += $" | Exception: {Exception.GetType().Name}: {Exception.Message}";
                if (!string.IsNullOrWhiteSpace(Exception.StackTrace))
                {
                    msg += Environment.NewLine + Exception.StackTrace;
                }
            }
            return $"[{timestamp}] [{level}] {msg}";
        }
    }
}
