using System;
using System.Threading.Tasks;

namespace ReceiptPrinter.Logging.Interfaces
{
    /// <summary>
    /// Defines the contract for logging operations.
    /// Supports multiple log levels with optional exception details.
    /// </summary>
    public interface ILogger
    {
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception ex = null);
        void Fatal(string message, Exception ex = null);

        // Async versions of the methods
        Task DebugAsync(string message);
        Task InfoAsync(string message);
        Task WarningAsync(string message);
        Task ErrorAsync(string message, Exception ex = null);
        Task FatalAsync(string message, Exception ex = null);
    }
}
