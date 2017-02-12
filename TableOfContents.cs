using System.Collections.Generic;
using System.IO;

namespace Librarian
{
    class TableOfContents
    {
        public static readonly int CONTENTS_OFFSET = 0x20;

        List<TzarFileInfo> m_tzarFiles;
        Book m_book;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public TableOfContents (Book book)
        {
            m_book = book;

            DecompressAndParse ();            
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public int Count
        {
            get { return m_tzarFiles.Count; }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public TzarFileInfo this[int index]
        {
            get { return m_tzarFiles[index]; }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void DecompressAndParse ()
        {
            Chapter contentsChapter = m_book.ChapterList[0];
            byte[] contentDecompressed = new byte [m_book.PageSize];
            var memStream = new MemoryStream (contentDecompressed);
            Lzss.Decompress (m_book, contentsChapter, memStream);

            using (var decompressedStream = new MemoryStream (contentDecompressed))
            {
                decompressedStream.Seek (CONTENTS_OFFSET, SeekOrigin.Begin);
                var decompressedContents = new BinaryReader (decompressedStream);

                int numberOfFiles = decompressedContents.ReadInt32 ();
                int archiveSize   = decompressedContents.ReadInt32 ();

                m_tzarFiles = new List<TzarFileInfo> (numberOfFiles);

                TzarFileInfo previousFileInfo = null;

                for (int i = 0; i < numberOfFiles; i++)
                {
                    int     nameLength         = decompressedContents.ReadByte ();
                    int     nameReuseLength    = decompressedContents.ReadByte ();
                    string  filePath           = previousFileInfo != null ? previousFileInfo.Path.Substring (0, nameReuseLength) : "";
                    filePath += new string (decompressedContents.ReadChars (nameLength - nameReuseLength));

                    int fileOffset = decompressedContents.ReadInt32 ();
                    int fileLength = decompressedContents.ReadInt32 ();

                    var tzarFileInfo = new TzarFileInfo (filePath, nameLength, fileLength, fileOffset);
                    previousFileInfo = tzarFileInfo;

                    m_tzarFiles.Add (tzarFileInfo);
                }
            }
        }
    }
}
