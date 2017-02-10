using System.IO;

namespace Librarian
{
    class Book
    {
        public static readonly int HEADER_LENGTH = 0xE;

        public string   Path;
        public string   CompressionType;
        public uint     PageSize;
        public int      BitLengths;
        public int      SizeCompressed;
        public uint     SizeDecompressed;
        public int      UnkArgument;

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
                SizeDecompressed = reader.ReadUInt32 ();
                PageSize         = reader.ReadUInt32 ();
                BitLengths       = Utils.GetBitLenghts (reader.ReadByte ());
                UnkArgument      = reader.ReadByte ();
            }

            // Order is of importance here (TableOfContents depends on ChapterList)
            ChapterList     = new ChapterList (this);
            TableOfContents = new TableOfContents (this);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void PrintBasicInfo ()
        {
            // TODO: Push/Pop indent
            System.Console.WriteLine ();
            DebugUtils.PrintHex (SizeCompressed, 8, "Compressed book size", 2);
            DebugUtils.PrintHex (SizeDecompressed, 8, "Decompressed book size", 2);
            DebugUtils.PrintHex (PageSize, 8, "Page Size", 3);
            DebugUtils.PrintHex (BitLengths, 2, "Bit Lengths", 3);
            DebugUtils.PrintHex (UnkArgument, 2, "Unk arg", 3);
        }
    }
}
