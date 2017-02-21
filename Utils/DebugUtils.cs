using System;

namespace Librarian.Utils
{
    public static class DebugUtils
    {
        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static void PrintHex (object o, string description = "")
        {
            if (string.IsNullOrEmpty (description))
                Console.WriteLine (string.Format ("0x{0:X}", o));
            else
                Console.WriteLine (string.Format ("{0}: 0x{0:X}"), o);
        }
    }
}