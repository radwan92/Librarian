using System;
using System.IO;

namespace Librarian.Wdt
{
    public class PackFile : IDisposable
    {
        public PackContents Contents { get; private set; }

        MemoryStream m_packStream;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static PackFile CreateFromFile (string filePath)
        {
            byte[] packFile = File.ReadAllBytes (filePath);

            var packFileStream = new MemoryStream (packFile);

            return new PackFile (packFileStream);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static PackFile CreateFromWdtFile (WdtFile wdtFile)
        {
            var packFileStream = new MemoryStream (wdtFile.SizeDecompressed);

            for (int i = 0; i < wdtFile.ChapterList.Count; i++)
            {
                var chapter = wdtFile.ChapterList[i];

                byte[]  decompressedChapter = new byte[wdtFile.PageSize];
                var     chapterMemoryStream = new MemoryStream (decompressedChapter);
                int     decompressedSize    = WdtDecompressor.DecompressChapter (wdtFile, chapter, chapterMemoryStream);

                decompressedSize = Math.Min (wdtFile.PageSize, decompressedSize);

                packFileStream.Write (decompressedChapter, 0, decompressedSize);
            }

            return new PackFile (packFileStream);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        PackFile (MemoryStream packStream)
        {
            m_packStream = packStream;
            m_packStream.Seek (0, SeekOrigin.Begin);

            Contents = PackContents.CreateFromPackFile (m_packStream);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public byte[] RetrieveTzarFile (PackTzarFile tzarFile)
        {
            byte[] fileBuffer = new byte [tzarFile.Size];

            m_packStream.Seek (tzarFile.Offset, SeekOrigin.Begin);
            m_packStream.Read (fileBuffer, 0, tzarFile.Size);

            return fileBuffer;
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void Dispose ()
        {
            if (m_packStream != null)
            {
                m_packStream.Dispose ();
                GC.SuppressFinalize (this);
            }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        ~PackFile ()
        {
            Dispose ();
        }
    }
}