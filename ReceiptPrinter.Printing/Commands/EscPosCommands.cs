// ReceiptPrinter.Printing/Commands/EscPosCommands.cs
using System.Text;

namespace ReceiptPrinter.Printing.Commands
{
    public static class EscPosCommands
    {
        public static readonly byte[] Initialize = { 0x1B, 0x40 };
        public static readonly byte[] LineFeed = { 0x0A };
        public static readonly byte[] PartialCut = { 0x1D, 0x56, 0x01 };

        public static byte[] AlignLeft() { return new byte[] { 0x1B, 0x61, 0x00 }; }
        public static byte[] AlignCenter() { return new byte[] { 0x1B, 0x61, 0x01 }; }
        public static byte[] AlignRight() { return new byte[] { 0x1B, 0x61, 0x02 }; }

        public static byte[] BoldOn() { return new byte[] { 0x1B, 0x45, 0x01 }; }
        public static byte[] BoldOff() { return new byte[] { 0x1B, 0x45, 0x00 }; }

        public static byte[] SetFontSize(byte size)
        {
            // ESC/POS font scaling (height * width)
            byte n;
            switch (size)
            {
                case 1:
                    n = 0x10; // Double height
                    break;
                case 2:
                    n = 0x01; // Double width
                    break;
                case 3:
                    n = 0x11; // Double height + width
                    break;
                default:
                    n = 0x00; // Normal
                    break;
            }

            return new byte[] { 0x1D, 0x21, n };
        }

        public static byte[] DoubleLineSeparator(int width = 42)
        {
            return Encoding.ASCII.GetBytes(new string('═', width) + "\n");
        }

        public static byte[] SingleLineSeparator(int width = 42)
        {
            return Encoding.ASCII.GetBytes(new string('─', width) + "\n");
        }

        public static byte[] Text(string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        // Map Windows font size → ESC/POS scale
        public static byte MapFontSize(int fontSize)
        {
            if (fontSize <= 10) return 0;
            if (fontSize <= 15) return 1;
            return 3;
        }
    }
}
