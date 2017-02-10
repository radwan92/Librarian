using System;
using System.IO;

namespace Librarian
{
    class LzssDecompressor
    {
        public static void Decompress (byte[] decompressBuffer, Chapter sourceChapter)
        {
            int decompLowPartLength = decompressBuffer.Length - 1 - (int)sourceChapter.Size;

            // TODO: Cleanup gotos -> decompose the method
            var headerHighStream       = new MemoryStream (decompressBuffer);
            var headerLowStream        = new MemoryStream (decompressBuffer);
            var binaryHeaderHighReader = new BinaryReader (headerHighStream);
            var binaryHeaderLowWriter  = new BinaryWriter (headerLowStream);
            {
                ushort      shifter           = 0;
                uint        processedValue    = 0;
                int         bytesDecompressed = 0;
                bool        carryFlag         = false;
                Func<bool>  isEOF             = () => bytesDecompressed >= sourceChapter.Size - 1;

                Console.WriteLine ();

                mainPart:
                while (!isEOF ())
                {
                    headerHighStream.Seek (decompLowPartLength + bytesDecompressed, SeekOrigin.Begin);

                    processedValue = BinaryUtils.SwapBytes (binaryHeaderHighReader.ReadUInt32 ());

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
                        byte someByte = decompressBuffer[binaryHeaderLowWriter.BaseStream.Position + helper - 0x1000];
                        binaryHeaderLowWriter.Write (someByte);
                    }
                }


                processedValue++;

                while (processedValue > 0)
                {
                    byte someByte = decompressBuffer[binaryHeaderLowWriter.BaseStream.Position + helper - 0x1000];
                    binaryHeaderLowWriter.Write (someByte);
                    someByte = decompressBuffer[binaryHeaderLowWriter.BaseStream.Position - 1 + helper - 0xFFF];
                    binaryHeaderLowWriter.Write (someByte);
                    processedValue--;
                }

                if (!isEOF ())
                    goto mainPart;

                endOfFile:
                Console.WriteLine ("I LOVE GOTO xD");
            }
        }
    }
}