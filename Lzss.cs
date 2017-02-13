using System.IO;

namespace Librarian
{
    class Lzss
    {
        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static byte[] GetFile (TzarFileInfo fileInfo, Book book)
        {
            byte[] fileBuffer = new byte[fileInfo.Size];

            int fileStartChapter = fileInfo.Offset / book.PageSize;
            int fileStartOffset  = fileInfo.Offset % book.PageSize;
            int fileChapterSpan  = (fileInfo.Size + fileStartOffset) / book.PageSize + 1;

            byte[] chaptersBuffer = new byte[fileChapterSpan * book.PageSize];
            var chaptersStream = new MemoryStream (chaptersBuffer);

            for (int i = 0; i < fileChapterSpan; i++)
                Decompress (book, book.ChapterList[fileStartChapter + i], chaptersStream);

            chaptersStream.Seek (fileStartOffset, SeekOrigin.Begin);
            chaptersStream.Read (fileBuffer, 0, fileInfo.Size);

            return fileBuffer;
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static int Decompress (Book book, Chapter chapter, Stream outStream)
        {
            int decompressedSize;

            var compressedBuffer = new byte[chapter.Size + 16];  // Additional safety bytes for LZSS
            using (var bookStream = new FileStream (book.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bookStream.Seek (chapter.StartPosition, SeekOrigin.Begin);
                bookStream.Read (compressedBuffer, 0, chapter.Size);
            }

            // TODO: Cleanup gotos; decompose the method
            var compressedPartStream      = new MemoryStream (compressedBuffer);
            var compressedReader          = new BinaryReader (compressedPartStream);
            var decompressedWriter        = new BinaryWriter (outStream);
            {
                ushort      shifter               = 0;
                uint        processedValue        = 0;
                int         bytesDecompressed     = 0;
                bool        carryFlag             = false;
                long        outStreamBasePosition = outStream.Position;

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

                    if ((outStream.Position - outStreamBasePosition) >= book.PageSize)
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

                        decompressedWriter.Seek ((int)(outStreamBasePosition + processedValue) + 2, SeekOrigin.Current);

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
                        long outStreamPosition = outStream.Position;
                        outStream.Seek (outStreamPosition + helper - 0x1000, SeekOrigin.Begin);
                        byte someByte = (byte)outStream.ReadByte ();
                        outStream.Seek (outStreamPosition, SeekOrigin.Begin);
                        decompressedWriter.Write (someByte);
                    }
                }

                processedValue++;

                while (processedValue > 0)
                {
                    long outStreamPosition = outStream.Position;
                    outStream.Seek (outStreamPosition + helper - 0x1000, SeekOrigin.Begin);
                    byte copiedByte = (byte)outStream.ReadByte ();
                    outStream.Seek (outStreamPosition, SeekOrigin.Begin);
                    decompressedWriter.Write (copiedByte);

                    outStreamPosition = outStream.Position;
                    outStream.Seek (outStreamPosition + helper - 0x1000, SeekOrigin.Begin);
                    copiedByte = (byte)outStream.ReadByte ();
                    outStream.Seek (outStreamPosition, SeekOrigin.Begin);
                    decompressedWriter.Write (copiedByte);

                    processedValue--;
                }

                if (bytesDecompressed < chapter.Size - 1)
                    goto mainPart;

                endOfFile:
                    decompressedSize = (int)(outStream.Position - outStreamBasePosition);
            }

            compressedPartStream.Dispose ();

            return decompressedSize;
        }
    }
}