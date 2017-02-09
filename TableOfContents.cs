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
            using (var fileStream = new FileStream (m_book.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var reader = new BinaryReader (fileStream);
                fileStream.Seek (Book.HEADER_LENGTH, SeekOrigin.Begin);  // Get past header

                //byte[] contentCompressed = new byte[CompressedSizeRounded * 4];
                //reader.Read (contentCompressed, 0, CompressedSize);

                //// Append WDT file size (compressed) to the content buffer [remember that it was rounded/extended to/by 2 bytes]
                //BitConverter.GetBytes (book.CompressedSize).CopyTo (contentCompressed, CompressedSize);

                // Had to hardcode size + 1 and the 0xAB at the end due to c++ malloc (to fully replicate arch.exe behaviour)
                // More on this: https://msdn.microsoft.com/en-us/library/ms220938%28v=vs.80%29.aspx?f=255&MSPPError=-2147217396
                byte[] contentDecompressed = new byte[SizeDecompressed + 1];
                contentDecompressed[SizeDecompressed] = 0xAB;


                //var compressedStream = new MemoryStream (contentCompressed);
                //var compressedReader = new BinaryReader (compressedStream);

                int chapterStart      = reader.ReadInt32 ();
                int chapterEnd        = reader.ReadInt32 ();
                int chapterSize       = chapterEnd - chapterStart;
                int chapterCopyOffset = SizeDecompressed - chapterSize;

                DebugUtils.PrintHex (chapterCopyOffset, 8, "Chapter copy offset", 1);

                fileStream.Seek (chapterSize, SeekOrigin.Begin);
                fileStream.Read (contentDecompressed, chapterCopyOffset, chapterSize);
            }
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
