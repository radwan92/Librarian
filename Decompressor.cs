using System.IO;
using System;
using System.Collections.Generic;

namespace Librarian
{
    class Decompressor
    {
        string m_decompressedFile;

        WdtFile         m_wdtFile = new WdtFile ();
        WdtContentList  m_wdtContents;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        // TODO: When we get some idea on how this stuff works, decompose this method
        public void DecompressWdt (string filePath)
        {
            m_decompressedFile = filePath;

            using (var fileStream = File.OpenRead (filePath))
            {
                m_wdtFile.ReadInHeader (new BinaryReader (fileStream));
                m_wdtFile.PrintInfo ();

                m_wdtContents = new WdtContentList (m_wdtFile);
                m_wdtFile.PrintInfo ();


                // Read compressed header into memory
                byte[] header = new byte[extendedCompressedHeaderSize * 4];
                fileStream.Read (header, 0, baseCompHeadSizeBytes);

                // Append WDT file size to the header
                int wdtFileSize = (int)fileStream.Length;
                BitConverter.GetBytes (wdtFileSize).CopyTo (header, baseCompHeadSizeBytes);

                // Alloc decomp. header
                // ---
                // Had to hardcode size + 1 and the 0xAB at the end due to c++ malloc
                // More on this: https://msdn.microsoft.com/en-us/library/ms220938%28v=vs.80%29.aspx?f=255&MSPPError=-2147217396
                byte[] decompHeader = new byte[decompressedHeaderSize + 1];
                decompHeader[decompressedHeaderSize] = 0xAB;    

                // TODO: Find out what 0xF stands for (it's pushed to the method around there in arch)

                var compressedHeaderStream = new MemoryStream (header);
                var binaryCompHeaderStream = new BinaryReader (compressedHeaderStream);

                int size1                = binaryCompHeaderStream.ReadInt32 ();
                int size2                = binaryCompHeaderStream.ReadInt32 ();
                int decompHighPartLength = size2 - size1;
                int decompLowPartLength  = decompressedHeaderSize - decompHighPartLength;

                Console.WriteLine ();
                DebugUtils.PrintHex (size1, 8, "Size1");
                DebugUtils.PrintHex (size2, 8, "Size2");
                DebugUtils.PrintHex (decompHighPartLength, 8, "Diff1");
                DebugUtils.PrintHex (decompLowPartLength, 8, "Offset1", 1);

                // Read compressed header to the decompressed header
                // in preparation for the LZSS decompression
                fileStream.Seek (size1, SeekOrigin.Begin);
                fileStream.Read (decompHeader, decompLowPartLength, decompHighPartLength);

                compressedHeaderStream.Dispose ();

                int bitLengthsCoeff  = bitLengths - 0xA1;   // If bigger than 24 - failure
                int bitLengthsLookup = m_bitLengthsTable[bitLengthsCoeff];

                Console.WriteLine ();
                DebugUtils.PrintHex (bitLengths, 2, "Bit Lengths", 3);
                DebugUtils.PrintHex (bitLengthsCoeff, 2, "Bit Lengths Coeff");
                DebugUtils.PrintHex (bitLengthsLookup, 2, "Bit Lengths Lookup");

                /* ================================================================================================================================== */
                // LZSS DECOMPRESSION
                /* ================================================================================================================================== */
                // TODO: Cleanup gotos -> decompose the method
                var headerHighStream       = new MemoryStream (decompHeader);
                var headerLowStream        = new MemoryStream (decompHeader);
                var binaryHeaderHighReader = new BinaryReader (headerHighStream);
                var binaryHeaderLowWriter  = new BinaryWriter (headerLowStream);
                {
                    ushort      shifter           = 0;
                    uint        processedValue    = 0;
                    int         bytesDecompressed = 0;
                    bool        carryFlag         = false;
                    Func<bool>  isEOF             = () => bytesDecompressed >= decompHighPartLength - 1;

                    Console.WriteLine ();

                mainPart:
                    while (!isEOF ())
                    {
                        headerHighStream.Seek (decompLowPartLength + bytesDecompressed, SeekOrigin.Begin);

                        processedValue = BinaryUtils.SwapBytes (binaryHeaderHighReader.ReadUInt32());

                        processedValue = processedValue << (byte)shifter;
                        bool isShiftCarry = (processedValue & 0x80000000) > 0;
                        processedValue = processedValue << 1;

                        if (!isShiftCarry)
                            break;

                        BinaryUtils.AddAndSetCarryFlag (ref shifter, 0x2001, ref carryFlag);
                        bytesDecompressed += carryFlag ? 2 : 1;

                        shifter = (ushort)(shifter & 0xFF07);

                        processedValue = BinaryUtils.SwapBytes (processedValue);
                        binaryHeaderLowWriter.Write ((byte)processedValue);
                    }

                    BinaryUtils.AddAndSetCarryFlag (ref shifter, 0x2001, ref carryFlag);
                    BinaryUtils.AddWithCarry (ref bytesDecompressed, 2, carryFlag);
                    shifter = (ushort)(shifter & 0xFF07);

                    uint helper = (processedValue >> 0x14);

                    {
                        bool shrCarryFlag = false;
                        if ((helper & 0x80000) > 0)
                            shrCarryFlag = true;

                        if (shrCarryFlag)
                        {
                            processedValue &= 0xF0000;
                            processedValue = processedValue >> 0x10;

                            binaryHeaderLowWriter.Seek ((int)processedValue + 2, SeekOrigin.Current);

                            if (isEOF ())
                                goto endOfFile;
                            else
                                goto mainPart;
                        }
                    }


                    {
                        processedValue &= 0xF0000;

                        bool shrCarryFlag = false;
                        if ((processedValue & 0x10000) > 0)
                            shrCarryFlag = true;

                        processedValue = processedValue >> 0x11;

                        if (shrCarryFlag)
                        {
                            byte someByte = decompHeader[binaryHeaderLowWriter.BaseStream.Position + helper - 0x1000];
                            binaryHeaderLowWriter.Write (someByte);
                        }
                    }


                    processedValue++;

                    while (processedValue > 0)
                    {
                        byte someByte = decompHeader[binaryHeaderLowWriter.BaseStream.Position + helper - 0x1000];
                        binaryHeaderLowWriter.Write (someByte);
                        someByte = decompHeader[binaryHeaderLowWriter.BaseStream.Position - 1 + helper - 0xFFF];
                        binaryHeaderLowWriter.Write (someByte);
                        processedValue--;
                    }

                    if (!isEOF ())
                        goto mainPart;

                endOfFile:
                        Console.WriteLine ("I LOVE GOTO xD");
                }

                // Reading files from decompressed header

                headerHighStream.Seek (0x20, SeekOrigin.Begin);

                int numberOfFiles = binaryHeaderHighReader.ReadInt32 ();
                int archiveSize = binaryHeaderHighReader.ReadInt32 ();

                var files = new List<HmmsysPackFile> (numberOfFiles);

                HmmsysPackFile previousFile = null;
                for (int i = 0; i < numberOfFiles; i++)
                {
                    int     nameLength         = binaryHeaderHighReader.ReadByte ();
                    int     nameReuseLength    = binaryHeaderHighReader.ReadByte ();
                    string  fileName           = previousFile != null ? previousFile.Name.Substring (0, nameReuseLength) : "";
                    fileName += new string (binaryHeaderHighReader.ReadChars (nameLength - nameReuseLength));

                    int fileOffset = binaryHeaderHighReader.ReadInt32 ();
                    int fileLength = binaryHeaderHighReader.ReadInt32 ();

                    var packFile = new HmmsysPackFile (fileName, nameLength, nameReuseLength, fileLength, fileOffset);
                    previousFile = packFile;

                    files.Add (packFile);
                }

                headerLowStream.Dispose ();
                headerHighStream.Dispose ();
            }
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