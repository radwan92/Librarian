using System;
using System.Collections.Generic;
using System.IO;

namespace Librarian
{
    class TableOfContents
    {
        public static readonly int CONTENTS_OFFSET = 0x20;

        public List<TzarFileInfo> TzarFiles;

        Book m_book;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public TableOfContents (Book book)
        {
            m_book = book;

            ReadThenDecompressAndParse ();            
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void ReadThenDecompressAndParse ()
        {
            byte[]  contentDecompressed;
            Chapter contentsChapter;

            using (var fileStream = new FileStream (m_book.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var reader = new BinaryReader (fileStream);
                fileStream.Seek (Book.HEADER_LENGTH, SeekOrigin.Begin);  // Get past header

                // Had to hardcode size + 1 and the 0xAB at the end due to c++ malloc (to fully replicate arch.exe behaviour)
                // More on this: https://msdn.microsoft.com/en-us/library/ms220938%28v=vs.80%29.aspx?f=255&MSPPError=-2147217396
                // OR DO WE REALLY?
                contentDecompressed = new byte[m_book.ChapterBufferSize + 1];
                //contentDecompressed[m_book.ChapterBufferSize] = 0xAB;

                contentsChapter = m_book.ChapterList[0];
                int chapterCopyOffset = (int)(m_book.ChapterBufferSize - contentsChapter.Size);

                contentsChapter.PrintInfo ();
                DebugUtils.PrintHex (chapterCopyOffset, 8, "Chapter copy offset", 1);

                fileStream.Seek (contentsChapter.StartPosition, SeekOrigin.Begin);
                fileStream.Read (contentDecompressed, chapterCopyOffset, (int)contentsChapter.Size);
            }

            int unused; // TODO: Remove. Temporary solution
            LzssDecompressor.Decompress (contentDecompressed, contentsChapter, out unused);

            using (var decompressedStream = new MemoryStream (contentDecompressed))
            {
                decompressedStream.Seek (CONTENTS_OFFSET, SeekOrigin.Begin);
                var decompressedContents = new BinaryReader (decompressedStream);

                int numberOfFiles = decompressedContents.ReadInt32 ();
                int archiveSize   = decompressedContents.ReadInt32 ();

                TzarFiles = new List<TzarFileInfo> (numberOfFiles);

                TzarFileInfo previousFileInfo = null;

                for (int i = 0; i < numberOfFiles; i++)
                {
                    int     nameLength         = decompressedContents.ReadByte ();
                    int     nameReuseLength    = decompressedContents.ReadByte ();
                    string  fileName           = previousFileInfo != null ? previousFileInfo.Name.Substring (0, nameReuseLength) : "";
                    fileName += new string (decompressedContents.ReadChars (nameLength - nameReuseLength));

                    int fileOffset = decompressedContents.ReadInt32 ();
                    int fileLength = decompressedContents.ReadInt32 ();

                    var tzarFileInfo = new TzarFileInfo (fileName, nameLength, fileLength, fileOffset);
                    previousFileInfo = tzarFileInfo;

                    TzarFiles.Add (tzarFileInfo);
                }
            }
        }
    }
}
