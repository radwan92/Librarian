using System;
using System.IO;

namespace Librarian
{
    class TableOfContents
    {
        // Somewhat unknown constants
        static readonly int m_mlp = 9;
        static readonly int m_dir = 8;
        static readonly int m_uar = 2;

        public int SizeDecompressed;
        public int SizeCompressed;
        public int SizeCompressedRounded;

        Book m_book;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public TableOfContents (Book book)
        {
            m_book = book;

            CalculateSize ();
            ReadThenDecompressAndParse ();            
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void CalculateSize ()
        {
            SizeDecompressed = (int)(m_mlp * m_book.PageSize / m_dir + m_uar);
            SizeCompressed   = (int)((m_book.SizeDecompressed - 1) / m_book.PageSize) + 1;

            // Round to an additional 16 bytes
            SizeCompressedRounded = SizeCompressed / 16;
            SizeCompressedRounded = (++SizeCompressedRounded) * 16;
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void ReadThenDecompressAndParse ()
        {
            byte[] contentDecompressed;
            Chapter contentsChapter;

            using (var fileStream = new FileStream (m_book.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var reader = new BinaryReader (fileStream);
                fileStream.Seek (Book.HEADER_LENGTH, SeekOrigin.Begin);  // Get past header

                // Had to hardcode size + 1 and the 0xAB at the end due to c++ malloc (to fully replicate arch.exe behaviour)
                // More on this: https://msdn.microsoft.com/en-us/library/ms220938%28v=vs.80%29.aspx?f=255&MSPPError=-2147217396
                contentDecompressed = new byte[SizeDecompressed + 1];
                contentDecompressed[SizeDecompressed] = 0xAB;

                contentsChapter = m_book.ChapterList[0];
                int chapterCopyOffset = SizeDecompressed - (int)contentsChapter.Size;

                contentsChapter.PrintInfo ();
                DebugUtils.PrintHex (chapterCopyOffset, 8, "Chapter copy offset", 1);

                fileStream.Seek (contentsChapter.StartPosition, SeekOrigin.Begin);
                fileStream.Read (contentDecompressed, chapterCopyOffset, (int)contentsChapter.Size);
            }

            LzssDecompressor.Decompress (contentDecompressed, contentsChapter);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void PrintSizeInfo ()
        {
            Console.WriteLine ();
            DebugUtils.PrintHex (SizeDecompressed, 8, "Decompressed content list size", 1);
            DebugUtils.PrintHex (SizeCompressed, 8, "Compressed content list size", 1);
            DebugUtils.PrintHex (SizeCompressedRounded, 8, "Rounded compr. content list size", 1);
            DebugUtils.PrintHex (SizeCompressed * 4, 8, "Content list Size [bytes]");
            DebugUtils.PrintHex (SizeCompressedRounded * 4, 8, "Content list Extended size [bytes]", 1);
        }
    }
}
