namespace Librarian
{
    class Decompressor
    {
        Book m_book;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void DecompressWdt (string filePath)
        {
            m_book = new Book (filePath);
            
            ///* ================================================================================================================================== */
            //// LZSS DECOMPRESSION
            ///* ================================================================================================================================== */
            //// TODO: Cleanup gotos -> decompose the method
            //var headerHighStream       = new MemoryStream (decompHeader);
            //var headerLowStream        = new MemoryStream (decompHeader);
            //var binaryHeaderHighReader = new BinaryReader (headerHighStream);
            //var binaryHeaderLowWriter  = new BinaryWriter (headerLowStream);
            //{
            //    ushort      shifter           = 0;
            //    uint        processedValue    = 0;
            //    int         bytesDecompressed = 0;
            //    bool        carryFlag         = false;
            //    Func<bool>  isEOF             = () => bytesDecompressed >= decompHighPartLength - 1;

            //    Console.WriteLine ();

            //mainPart:
            //    while (!isEOF ())
            //    {
            //        headerHighStream.Seek (decompLowPartLength + bytesDecompressed, SeekOrigin.Begin);

            //        processedValue = BinaryUtils.SwapBytes (binaryHeaderHighReader.ReadUInt32());

            //        processedValue = processedValue << (byte)shifter;
            //        bool isShiftCarry = (processedValue & 0x80000000) > 0;
            //        processedValue = processedValue << 1;

            //        if (!isShiftCarry)
            //            break;

            //        BinaryUtils.AddAndSetCarryFlag (ref shifter, 0x2001, ref carryFlag);
            //        bytesDecompressed += carryFlag ? 2 : 1;

            //        shifter = (ushort)(shifter & 0xFF07);

            //        processedValue = BinaryUtils.SwapBytes (processedValue);
            //        binaryHeaderLowWriter.Write ((byte)processedValue);
            //    }

            //    BinaryUtils.AddAndSetCarryFlag (ref shifter, 0x2001, ref carryFlag);
            //    BinaryUtils.AddWithCarry (ref bytesDecompressed, 2, carryFlag);
            //    shifter = (ushort)(shifter & 0xFF07);

            //    uint helper = (processedValue >> 0x14);

            //    {
            //        bool shrCarryFlag = false;
            //        if ((helper & 0x80000) > 0)
            //            shrCarryFlag = true;

            //        if (shrCarryFlag)
            //        {
            //            processedValue &= 0xF0000;
            //            processedValue = processedValue >> 0x10;

            //            binaryHeaderLowWriter.Seek ((int)processedValue + 2, SeekOrigin.Current);

            //            if (isEOF ())
            //                goto endOfFile;
            //            else
            //                goto mainPart;
            //        }
            //    }


            //    {
            //        processedValue &= 0xF0000;

            //        bool shrCarryFlag = false;
            //        if ((processedValue & 0x10000) > 0)
            //            shrCarryFlag = true;

            //        processedValue = processedValue >> 0x11;

            //        if (shrCarryFlag)
            //        {
            //            byte someByte = decompHeader[binaryHeaderLowWriter.BaseStream.Position + helper - 0x1000];
            //            binaryHeaderLowWriter.Write (someByte);
            //        }
            //    }


            //    processedValue++;

            //    while (processedValue > 0)
            //    {
            //        byte someByte = decompHeader[binaryHeaderLowWriter.BaseStream.Position + helper - 0x1000];
            //        binaryHeaderLowWriter.Write (someByte);
            //        someByte = decompHeader[binaryHeaderLowWriter.BaseStream.Position - 1 + helper - 0xFFF];
            //        binaryHeaderLowWriter.Write (someByte);
            //        processedValue--;
            //    }

            //    if (!isEOF ())
            //        goto mainPart;

            //endOfFile:
            //        Console.WriteLine ("I LOVE GOTO xD");
            //}

            //// Reading files from decompressed header

            //headerHighStream.Seek (0x20, SeekOrigin.Begin);

            //int numberOfFiles = binaryHeaderHighReader.ReadInt32 ();
            //int archiveSize = binaryHeaderHighReader.ReadInt32 ();

            //var files = new List<HmmsysPackFile> (numberOfFiles);

            //HmmsysPackFile previousFile = null;
            //for (int i = 0; i < numberOfFiles; i++)
            //{
            //    int     nameLength         = binaryHeaderHighReader.ReadByte ();
            //    int     nameReuseLength    = binaryHeaderHighReader.ReadByte ();
            //    string  fileName           = previousFile != null ? previousFile.Name.Substring (0, nameReuseLength) : "";
            //    fileName += new string (binaryHeaderHighReader.ReadChars (nameLength - nameReuseLength));

            //    int fileOffset = binaryHeaderHighReader.ReadInt32 ();
            //    int fileLength = binaryHeaderHighReader.ReadInt32 ();

            //    var packFile = new HmmsysPackFile (fileName, nameLength, nameReuseLength, fileLength, fileOffset);
            //    previousFile = packFile;

            //    files.Add (packFile);
            //}

            //headerLowStream.Dispose ();
            //headerHighStream.Dispose ();
        }
    }
}

class HmmsysPackFile
{
    public int      NameLength      { get; set; }
    public int      NameReuseLength { get; set; }
    public string   Name            { get; set; }
    public int      Length          { get; set; }
    public int      Offset          { get; set; }

    public HmmsysPackFile (string name, int nameLength, int nameReuseLength, int length, int offset)
    {
        Name            = name;
        NameLength      = nameLength;
        NameReuseLength = nameReuseLength;
        Length          = length;
        Offset          = offset;
    }
}