using System.IO;

namespace Librarian
{
    class Lzss
    {
        public static byte[] GetFile (TzarFileInfo fileInfo, Book book)
        {
            byte[] fileBuffer = new byte[fileInfo.Size];

            int startChapter = fileInfo.Offset / book.PageSize;
            int chapterCount = fileInfo.Size / book.PageSize;

            // This is a very crude, test implementation.

            return fileBuffer;
        }

        public static byte[] Decompress (Book book, Chapter chapter, out int byteCount)
        {
            byte[] decompressedBuffer = new byte[book.PageSize]; // TEMP: We will replase buffer with a sink stream

            var compressedBuffer = new byte[chapter.Size + 40];  // Additional safety bytes for LZSS
            using (var bookStream = new FileStream (book.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bookStream.Seek (chapter.StartPosition, SeekOrigin.Begin);
                bookStream.Read (compressedBuffer, 0, chapter.Size);
            }

            // TODO: Cleanup gotosl; decompose the method
            var compressedPartStream = new MemoryStream (compressedBuffer);
            var decompressedPartStream = new MemoryStream (decompressedBuffer);

            var compressedReader          = new BinaryReader (compressedPartStream);
            var decompressedWriter        = new BinaryWriter (decompressedPartStream);
            {
                ushort      shifter           = 0;
                uint        processedValue    = 0;
                int         bytesDecompressed = 0;
                bool        carryFlag         = false;

                mainPart:
                while (bytesDecompressed < chapter.Size - 1)
                {
                    compressedPartStream.Seek (bytesDecompressed, SeekOrigin.Begin);

                    processedValue = BinaryUtils.SwapBytes (compressedReader.ReadUInt32 ());

                    processedValue = processedValue << (byte)shifter;
                    bool isShiftCarry = (processedValue & 0x80000000) > 0;
                    processedValue = processedValue << 1;

                    if (!isShiftCarry)
                        break;

                    BinaryUtils.AddAndSetCarryFlag (ref shifter, 0x2001, ref carryFlag);
                    bytesDecompressed += carryFlag ? 2 : 1;

                    shifter = (ushort)(shifter & 0xFF07);

                    processedValue = BinaryUtils.SwapBytes (processedValue);
                    decompressedWriter.Write ((byte)processedValue);

                    if (decompressedPartStream.Position >= book.PageSize)
                        goto endOfFile;
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

                        decompressedWriter.Seek ((int)processedValue + 2, SeekOrigin.Current);

                        if (bytesDecompressed < chapter.Size - 1)
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
                        byte someByte = decompressedBuffer[decompressedPartStream.Position + helper - 0x1000];
                        decompressedWriter.Write (someByte);
                    }
                }

                processedValue++;

                while (processedValue > 0)
                {
                    byte someByte = decompressedBuffer[decompressedPartStream.Position + helper - 0x1000];
                    decompressedWriter.Write (someByte);
                    someByte = decompressedBuffer[decompressedPartStream.Position - 1 + helper - 0xFFF];
                    decompressedWriter.Write (someByte);
                    processedValue--;
                }

                if (bytesDecompressed < chapter.Size - 1)
                    goto mainPart;

                endOfFile:
                    byteCount = (int)decompressedPartStream.Position;
            }

            compressedPartStream.Dispose ();
            decompressedPartStream.Dispose ();

            return decompressedBuffer;
        }
    }
}