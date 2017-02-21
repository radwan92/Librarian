using System;
using Librarian.Utils;

namespace Librarian.Wdt
{
    public struct Chapter
    {
        public readonly int StartPosition;
        public readonly int EndPosition;
        public readonly int Size;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public Chapter (int startPosition, int endPosition)
        {
            StartPosition = startPosition;
            EndPosition   = endPosition;
            Size          = endPosition - startPosition;
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void PrintInfo ()
        {
            Console.WriteLine ();
            DebugUtils.PrintHex (StartPosition, "Chapter start");
            DebugUtils.PrintHex (EndPosition, "Chapter end");
            DebugUtils.PrintHex (Size, "Chapter size");
        }
    }
}
