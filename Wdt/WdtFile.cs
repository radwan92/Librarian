using System;
using System.IO;
using Librarian.Utils;

namespace Librarian.Wdt
{
    public class WdtFile
    {
        public static readonly int HEADER_LENGTH = 0xE;

        // Somewhat unknown constants
        static readonly int m_mlp = 9;
        static readonly int m_dir = 8;
        static readonly int m_uar = 2;

        public readonly string   Path;
        public readonly string   CompressionType;
        public readonly int      PageSize;
        public readonly int      SizeCompressed;
        public readonly int      SizeDecompressed;
        public readonly int      ChapterBufferSize;

        //public WdtContents  Contents    { get; private set; }
        public ChapterList ChapterList { get; private set; }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static WdtFile CreateFromFile (string filePath)
        {
            return new WdtFile (filePath);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        WdtFile (string filePath)
        {
            Path = filePath;

            using (var fileStream = new FileStream (filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var reader = new BinaryReader (fileStream);

                CompressionType  = new string (reader.ReadChars (4));
                SizeCompressed   = (int)fileStream.Length;
                SizeDecompressed = reader.ReadInt32 ();
                PageSize         = reader.ReadInt32 ();

                // There are 2 additional bytes in the header - bit lengths and some unknown argument.
                // Both seem to be unused, so we skip them
            }

            ChapterBufferSize = m_mlp * PageSize / m_dir + m_uar;

            // Order is of importance here (WdtContents depend on ChapterList)
            ChapterList = new ChapterList (this);
            //Contents    = new WdtContents (this);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void PrintBasicInfo ()
        {
            Console.WriteLine ();
            DebugUtils.PrintHex (SizeCompressed, "Compressed wdtFile size");
            DebugUtils.PrintHex (SizeDecompressed, "Decompressed wdtFile size");
            DebugUtils.PrintHex (PageSize, "Page Size");
            DebugUtils.PrintHex (ChapterBufferSize, "Chapter buffer size");
        }
    }
}
