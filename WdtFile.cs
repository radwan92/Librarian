using System.IO;

namespace Librarian
{
    class WdtFile
    {
        public string   Path;
        public string   CompressionType;
        public uint     PageSize;
        public int      BitLengths;
        public uint     DecompressedSize;

        public int UnkArgument;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void ReadInHeader (BinaryReader reader)
        {
            // TODO: Try/Catch maybe?

            CompressionType  = new string (reader.ReadChars (4));
            DecompressedSize = reader.ReadUInt32 ();
            PageSize         = reader.ReadUInt32 ();
            BitLengths       = DecompressionUtils.GetBitLenghts (reader.ReadByte ());
            UnkArgument      = reader.ReadByte ();
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void PrintInfo ()
        {
            // TODO: Push/Pop indent
            DebugUtils.PrintHex (DecompressedSize, 8, "Decompressed WDT size", 2);
            DebugUtils.PrintHex (PageSize, 8, "Page Size", 3);
            DebugUtils.PrintHex (BitLengths, 2, "Bit Lengths", 3);
            DebugUtils.PrintHex (UnkArgument, 2, "Unk arg", 3);
        }
    }
}
