using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Librarian.Wdt
{
    public class WdtContents
    {
        public static readonly int CONTENTS_OFFSET = 0x20;

        List<TzarFile>  m_tzarFiles;
        WdtFile         m_wdtFile;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public WdtContents (WdtFile wdtFile)
        {
            m_wdtFile = wdtFile;

            DecompressAndParse ();            
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public int Count
        {
            get { return m_tzarFiles.Count; }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public TzarFile this[int index]
        {
            get { return m_tzarFiles[index]; }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void DecompressAndParse ()
        {
            Chapter contentsChapter     = m_wdtFile.ChapterList[0];
            byte[]  contentDecompressed = new byte [m_wdtFile.PageSize];
            var     memStream           = new MemoryStream (contentDecompressed);

            WdtDecompressor.DecompressChapter (m_wdtFile, contentsChapter, memStream);

            using (var decompressedStream = new MemoryStream (contentDecompressed))
            {
                decompressedStream.Seek (CONTENTS_OFFSET, SeekOrigin.Begin);
                var decompressedContents = new BinaryReader (decompressedStream);

                int numberOfFiles = decompressedContents.ReadInt32 ();
                int archiveSize   = decompressedContents.ReadInt32 ();

                m_tzarFiles = new List<TzarFile> (numberOfFiles);

                StringBuilder filePathBuilder = new StringBuilder (256);

                // Read first tzar file
                byte    nameLength         = decompressedContents.ReadByte ();
                byte    nameReuseLength    = decompressedContents.ReadByte ();

                filePathBuilder.Append (decompressedContents.ReadChars (nameLength));
                
                int fileOffset = decompressedContents.ReadInt32 ();
                int fileLength = decompressedContents.ReadInt32 ();

                TzarFile lastTzarFile = new TzarFile (filePathBuilder.ToString(), nameLength, fileLength, fileOffset);

                // Read subsequent tzar files
                for (int i = 0; i < numberOfFiles; i++)
                {
                    nameLength         = decompressedContents.ReadByte ();
                    nameReuseLength    = decompressedContents.ReadByte ();

                    filePathBuilder.Remove (nameReuseLength, filePathBuilder.Length - nameReuseLength);
                    filePathBuilder.Append (decompressedContents.ReadChars (nameLength - nameReuseLength));

                    fileOffset = decompressedContents.ReadInt32 ();
                    fileLength = decompressedContents.ReadInt32 ();

                    lastTzarFile = new TzarFile (filePathBuilder.ToString(), nameLength, fileLength, fileOffset);

                    m_tzarFiles.Add (lastTzarFile);
                }
            }
        }
    }
}
