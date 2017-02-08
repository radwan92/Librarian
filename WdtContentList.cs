namespace Librarian
{
    class WdtContentList
    {
        // Somewhat unknown constants
        static readonly int m_mlp = 9;
        static readonly int m_dir = 8;
        static readonly int m_uar = 2;

        public int DecompressedSize;
        public int CompressedSize;
        public int CompressedSizeRounded;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public WdtContentList (WdtFile wdtFile)
        {
            DecompressedSize = (int)(m_mlp * wdtFile.PageSize / m_dir + m_uar);
            CompressedSize   = (int)((wdtFile.DecompressedSize - 1) / wdtFile.PageSize) + 1;

            // Round to an additional 16 bytes
            CompressedSizeRounded = CompressedSize / 16;
            CompressedSizeRounded = (++CompressedSizeRounded) * 16;
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void PrintInfo ()
        {
            DebugUtils.PrintHex (DecompressedSize, 8, "Decompressed header size", 1);
            DebugUtils.PrintHex (CompressedSize, 8, "Compressed header size", 1);
            DebugUtils.PrintHex (CompressedSizeRounded, 8, "Rounded compr. header size", 1);
            DebugUtils.PrintHex (CompressedSize * 4, 8, "Header size [bytes]");
            DebugUtils.PrintHex (CompressedSizeRounded * 4, 8, "Extended header size [bytes]", 1);
        }
    }
}
