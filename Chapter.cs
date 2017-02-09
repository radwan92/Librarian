using System;

namespace Librarian
{
    struct Chapter
    {
        public uint StartPosition;
        public uint EndPosition;
        public uint Size;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public Chapter (uint startPosition, uint endPosition)
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
