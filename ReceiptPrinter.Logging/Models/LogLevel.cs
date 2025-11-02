namespace ReceiptPrinter.Logging.Models
{
    /// <summary>
    /// Log severity levels in ascending order of importance.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Detailed debugging information.
        /// </summary>
        DEBUG = 0,

        /// <summary>
        /// Informational messages.
        /// </summary>
        INFO = 1,

        /// <summary>
        /// Warning messages.
        /// </summary>
        WARNING = 2,

        /// <summary>
        /// Error messages.
        /// </summary>
        ERROR = 3,

        /// <summary>
        /// Critical errors that may cause the application to terminate.
        /// </summary>
        FATAL = 4
    }
}
