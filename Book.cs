using System;
using System.IO;

namespace Librarian
{
    class Book
    {
        public static readonly int HEADER_LENGTH = 0xE;

        // Somewhat unknown constants
        static readonly int m_mlp = 9;
        static readonly int m_dir = 8;
        static readonly int m_uar = 2;

        public string   Path;
        public string   CompressionType;
        public int      PageSize;
        public int      SizeCompressed;
        public int      SizeDecompressed;
        public int      ChapterBufferSize;

        public TableOfContents  TableOfContents;
        public ChapterList      ChapterList;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public Book (string filePath)
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

            // Order is of importance here (TableOfContents depends on ChapterList)
            ChapterList     = new ChapterList (this);
            TableOfContents = new TableOfContents (this);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void PrintBasicInfo ()
        {
            // TODO: Push/Pop indent
            Console.WriteLine ();
            DebugUtils.PrintHex (SizeCompressed, 8, "Compressed book size", 2);
            DebugUtils.PrintHex (SizeDecompressed, 8, "Decompressed book size", 2);
            DebugUtils.PrintHex (PageSize, 8, "Page Size", 3);
            DebugUtils.PrintHex (ChapterBufferSize, 8, "Chapter buffer size");
        }
    }
}
