using System;
using System.Windows.Forms;
using ReceiptPrinter.Logging;

namespace ReceiptPrinter.UI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Global exception handling
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.Run(new Forms.MainForm());
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            LogManager.GetLogger().Fatal("Thread exception", e.Exception);
            MessageBox.Show("An unexpected error occurred. See logs for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogManager.GetLogger().Fatal("Unhandled exception", e.ExceptionObject as Exception);
            MessageBox.Show("A critical error occurred. The application will close.", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}