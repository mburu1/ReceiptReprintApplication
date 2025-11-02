// ReceiptPrinter.Printing/Behaviors/IPrintBehavior.cs
using ReceiptPrinter.Core.Models;
using ReceiptPrinter.UI.Presenters;

namespace ReceiptPrinter.Printing.Behaviors
{
    /// <summary>
    /// Defines behavior for print button interactions
    /// </summary>
    public interface IPrintBehavior
    {
        /// <summary>
        /// Determines if the print button should be enabled
        /// </summary>
        bool CanEnablePrintButton(ReceiptData receiptData);

        /// <summary>
        /// Handles print button click logic
        /// </summary>
        void OnPrintClicked(MainFormPresenter presenter);
    }
}
