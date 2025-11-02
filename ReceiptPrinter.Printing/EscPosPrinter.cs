using ReceiptPrinter.Logging;
using ReceiptPrinter.Logging.Interfaces;
using ReceiptPrinter.Printing.Commands;
using ReceiptPrinter.Printing.Interfaces;
using ReceiptPrinter.Printing.Models;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ReceiptPrinter.Printing
{
    /// <summary>
    /// Implements direct ESC/POS printing to thermal printers.
    /// Uses Win32 spooler to send raw commands.
    /// </summary>
    public class EscPosPrinter : IPrinter
    {
        private readonly PrinterSettings _settings;
        private bool _disposed = false;

        private static readonly ILogger _logger = LogManager.GetLogger();

        // Win32 API declarations
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] ref DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, uint dwCount, out uint dwWritten);

        public EscPosPrinter(PrinterSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public bool Print(PrintJob job)
        {
            _logger.Info($"Initiating ESC/POS print to {_settings.PrinterName}");

            if (job == null || !job.IsValid())
            {
                _logger.Warning("Invalid PrintJob.");
                return false;
            }

            try
            {
                var builder = new PrinterCommandBuilder();
                byte[] commands = builder.Build(job);

                if (commands.Length == 0)
                {
                    _logger.Warning("Empty command buffer.");
                    return false;
                }

                return SendBytesToPrinter(_settings.PrinterName, commands);
            }
            catch (Exception ex)
            {
                _logger.Error("ESC/POS print failed", ex);
                return false;
            }
        }

        private bool SendBytesToPrinter(string printerName, byte[] data)
        {
            IntPtr hPrinter = IntPtr.Zero;
            var di = new DOCINFOA { pDocName = "Receipt Reprint", pDataType = "RAW" };
            bool success = false;
            uint dwWritten = 0;

            try
            {
                if (OpenPrinter(printerName.Normalize(), out hPrinter, IntPtr.Zero))
                {
                    if (StartDocPrinter(hPrinter, 1, ref di))
                    {
                        if (StartPagePrinter(hPrinter))
                        {
                            IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(data.Length);
                            Marshal.Copy(data, 0, pUnmanagedBytes, data.Length);

                            success = WritePrinter(hPrinter, pUnmanagedBytes, (uint)data.Length, out dwWritten);

                            Marshal.FreeCoTaskMem(pUnmanagedBytes);

                            EndPagePrinter(hPrinter);
                        }
                        EndDocPrinter(hPrinter);
                    }
                    ClosePrinter(hPrinter);
                }

                if (!success)
                {
                    int error = Marshal.GetLastWin32Error();
                    _logger.Error($"WritePrinter failed. Error: {error}");
                }
                else
                {
                    _logger.Info("Raw data sent successfully.");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.Error("Error sending bytes to printer", ex);
                return false;
            }
        }

        public bool IsAvailable()
        {
            try
            {
                System.Drawing.Printing.PrinterSettings.StringCollection installed =
                    System.Drawing.Printing.PrinterSettings.InstalledPrinters;

                if (installed == null) return false;

                // Convert StringCollection to string[]
                var printers = new string[installed.Count];
                installed.CopyTo(printers, 0);

                return printers.Contains(_settings.PrinterName);
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}