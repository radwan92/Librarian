using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Librarian.Wdt
{
    public class PackContents
    {
        public static readonly int CONTENTS_OFFSET = 0x20;

        List<PackTzarFile> m_packTzarFiles;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static PackContents CreateFromWdtFile (WdtFile wdtFile)
        {
            Chapter contentsChapter     = wdtFile.ChapterList[0];
            byte[]  contentDecompressed = new byte [wdtFile.PageSize];
            var     memStream           = new MemoryStream (contentDecompressed);

            WdtDecompressor.DecompressChapter (wdtFile, contentsChapter, memStream);

            PackContents packContents;
            using (var decompressedStream = new MemoryStream (contentDecompressed))
            {
                packContents = new PackContents (decompressedStream);
            }

            return packContents;
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static PackContents CreateFromPackFile (Stream packFileStream)
        {
            return new PackContents (packFileStream);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        PackContents (Stream packFileStream)
        {
            ParseFromPackFileStream (packFileStream);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public int Count
        {
            get { return m_packTzarFiles.Count; }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public PackTzarFile this[int index]
        {
            get { return m_packTzarFiles[index]; }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void ParseFromPackFileStream (Stream packFileStream)
        {
            packFileStream.Seek (CONTENTS_OFFSET, SeekOrigin.Begin);
            var decompressedContents = new BinaryReader (packFileStream);

            int numberOfFiles = decompressedContents.ReadInt32 ();
            int archiveSize   = decompressedContents.ReadInt32 ();

            m_packTzarFiles = new List<PackTzarFile> (numberOfFiles);

            StringBuilder filePathBuilder = new StringBuilder (256);

            // Read first tzar file
            byte    nameLength         = decompressedContents.ReadByte ();
            byte    nameReuseLength    = decompressedContents.ReadByte ();

            filePathBuilder.Append (decompressedContents.ReadChars (nameLength));
                
            int fileOffset = decompressedContents.ReadInt32 ();
            int fileLength = decompressedContents.ReadInt32 ();

            PackTzarFile lastTzarFile = new PackTzarFile (filePathBuilder.ToString(), nameLength, fileLength, fileOffset);

            // Read subsequent tzar files
            for (int i = 0; i < numberOfFiles - 1; i++)
            {
                nameLength         = decompressedContents.ReadByte ();
                nameReuseLength    = decompressedContents.ReadByte ();

                filePathBuilder.Remove (nameReuseLength, filePathBuilder.Length - nameReuseLength);
                filePathBuilder.Append (decompressedContents.ReadChars (nameLength - nameReuseLength));

                fileOffset = decompressedContents.ReadInt32 ();
                fileLength = decompressedContents.ReadInt32 ();

                lastTzarFile = new PackTzarFile (filePathBuilder.ToString(), nameLength, fileLength, fileOffset);

                m_packTzarFiles.Add (lastTzarFile);
            }
        }
    }
}
