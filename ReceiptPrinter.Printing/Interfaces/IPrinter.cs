using System;
using ReceiptPrinter.Printing.Models;

namespace ReceiptPrinter.Printing.Interfaces
{
    /// <summary>
    /// Defines the contract for printer implementations.
    /// Supports both ESC/POS and Windows Driver methods.
    /// </summary>
    public interface IPrinter : IDisposable
    {
        /// <summary>
        /// Executes the print operation for the given job.
        /// </summary>
        /// <param name="job">The PrintJob to process.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        bool Print(PrintJob job);

        /// <summary>
        /// Checks if the printer is available/online.
        /// </summary>
        /// <returns>True if available.</returns>
        bool IsAvailable();
    }
}