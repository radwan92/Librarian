using System;

namespace Librarian
{
    public static class DebugUtils
    {
        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static void PrintHex (object o, int padding, string description = "", int tabCount = 2)
        {
            // TODO: Padding instead of tabs. Proper formatting.
            Console.WriteLine (string.Format (string.IsNullOrEmpty (description) ? "" : (description + ": " + new string ('\t', tabCount)) + "0x{0:X" + padding + "}", o));
        }
    }
}