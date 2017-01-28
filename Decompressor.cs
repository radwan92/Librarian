using System.IO;
using System;

namespace Librarian
{
    public class Decompressor
    {
        // Somewhat unknown constants
        readonly int m_mlp = 9;
        readonly int m_dir = 8;
        readonly int m_uar = 2;

        string m_decompressedFile;

        // TODO: Transform into switch (or evaluation if possible)
        readonly byte[] m_bitLengthsTable =
        {
            0x00, 0x01, 0x02, 0x03,     0x04, 0x0F, 0x0F, 0x0F,     0x0F, 0x0F, 0x0F, 0x0F,     0x0F, 0x0F, 0x0F, 0x0F,
            0x05, 0x06, 0x07, 0x08,     0x09, 0x0F, 0x0F, 0x0F,     0x0F, 0x0F, 0x0F, 0x0F,     0x0F, 0x0F, 0x0F, 0x0F,
            0x0A, 0x0B, 0x0C, 0x0D,     0x0E
        };

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        // TODO: When we get some idea on how this stuff works, decompose this method
        public void Decompress (string filePath)
        {
            m_decompressedFile = filePath;

            using (var fileStream = File.OpenRead (filePath))
            {
                // Get base values from the file's header
                var     binaryReader    = new BinaryReader (fileStream);
                var     compressionType = new string (binaryReader.ReadChars (4));
                uint    decompWdtSize   = binaryReader.ReadUInt32 ();
                uint    coeff2          = binaryReader.ReadUInt32 ();
                byte    bitLengths      = binaryReader.ReadByte ();
                byte    unkArg          = binaryReader.ReadByte ();

                DebugUtils.PrintHex (decompWdtSize, 8, "Decomp WDT size", 2);
                DebugUtils.PrintHex (coeff2, 8, "Coeff2", 3);
                DebugUtils.PrintHex (bitLengths, 2, "Bit lengths", 3);
                DebugUtils.PrintHex (unkArg, 2, "Unk arg", 3);

                // TODO: What we refer to as header in the following part
                // is (as far as I'm concerned) a list of the files contained
                // in a given WDT. It should be distincted from the actual
                // header (the 2 ints and 2 bytes read above)

                // Calculating decompressed header size
                int decompressedHeaderSize = (int)(m_mlp * coeff2 / m_dir + m_uar);

                DebugUtils.PrintHex (decompressedHeaderSize, 8, "Decompressed header size", 1);

                // Calculate compressed header size
                int baseCompressedHeaderSize = (int)((decompWdtSize - 1) / coeff2) + 1;

                DebugUtils.PrintHex (baseCompressedHeaderSize, 8, "Compressed header size", 1);

                // Round to an additional 16 bytes
                int extendedCompressedHeaderSize = baseCompressedHeaderSize / 16;
                extendedCompressedHeaderSize ++;
                extendedCompressedHeaderSize = extendedCompressedHeaderSize * 16;

                DebugUtils.PrintHex (extendedCompressedHeaderSize, 8, "Extended compr. header size", 1);
                
                int baseCompHeadSizeBytes = baseCompressedHeaderSize * 4;

                DebugUtils.PrintHex (baseCompHeadSizeBytes, 8, "Header size [bytes]");
                DebugUtils.PrintHex (extendedCompressedHeaderSize * 4, 8, "Extended header size [bytes]", 1);

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
                headerLowStream.Dispose ();
                headerHighStream.Dispose ();
            }
        }
    }
}