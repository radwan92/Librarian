using System;

namespace Librarian
{
    struct Chapter
    {
        public int StartPosition;
        public int EndPosition;
        public int Size;

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
            DebugUtils.PrintHex (StartPosition, 8, "Chapter start");
            DebugUtils.PrintHex (EndPosition, 8, "Chapter end");
            DebugUtils.PrintHex (Size, 8, "Chapter size");
        }
    }
}
