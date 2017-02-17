using System;
using System.IO;

namespace Librarian
{
    class Rle
    {
        public static readonly int HEADER_SIZE = 0x26;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static void Decompress (string rlePath)
        {
            using (var rleStream = File.OpenRead (rlePath))
            {
                byte[] rleHeader = new byte[HEADER_SIZE];

                rleStream.Read (rleHeader, 0, HEADER_SIZE);

                int imageWidth = BitConverter.ToUInt16 (rleHeader, 0x14);
                int imageHeight = BitConverter.ToUInt16 (rleHeader, 0x16);

                // This is due to a BMP row being multiple of 4 Bytes (padded at the end if required)
                int roundedImageWidth = imageWidth;
                roundedImageWidth += 3;
                roundedImageWidth = roundedImageWidth >> 2;
                roundedImageWidth *= 2;
                roundedImageWidth *= 2;

                int imageSize = roundedImageWidth * imageHeight;
                byte[] imageBuffer = new byte[imageSize];

                int tableSize = imageHeight * 4;

                byte[] rleTable = new byte[tableSize + 4];

                rleStream.Read (rleTable, 0, tableSize);

                uint arg1 = BitConverter.ToUInt16 (rleHeader, 0x1A);
                uint arg2 = BitConverter.ToUInt16 (rleHeader, 0x1C);

                arg2 = arg2 << 0x10;
                arg2 |= arg1;

                Array.Copy (BitConverter.GetBytes (arg2), 0, rleTable, tableSize, 2);

                // ESP + 13  :: 1
                // ESP + 14  :: ??
                // ESP + 18  :: *Buffer
                // ESP + 1C  :: Buffer size
                // ESP + 7C  :: Buffer size
                // ESP + 24  :: x
                // ESP + 28  :: bufCounter
                // ESP + 30  :: 1
                // ESP + 38  :: *rleTable
                // ESP + 2C  :: *Buffer
                // ESP + 84  :: ?
                // ESP + 88  :: ?
                // ESP + 8C  :: ?
                // ESP + 98  :: imageWidth
                // ESP + 9A  :: imageHeight
                // ESP + 9C  :: imageType

                bool dl = false;
                int bufCounter = 0;

                for (int y = 0; y < imageHeight; y++)
                {
                    int valueDif = BitConverter.ToInt16 (rleTable, y * 4 + 4) - BitConverter.ToInt16 (rleTable, y * 4);

                    byte[] buffer = new byte[valueDif];
                    rleStream.Read (buffer, 0, valueDif);

                    bufCounter = 0;
                    dl         = true;

                    for (int bufferIndex = 0; bufferIndex < buffer.Length; bufferIndex++)
                    {
                        if (dl)
                        {
                            for (int x = 0; x < buffer[0]; x++)
                            {
                                int pix = (imageHeight - y - 1) * roundedImageWidth + bufCounter;
                                imageBuffer[pix] = 0;
                                bufCounter++;
                            } 

                            dl = false;
                            continue;
                        }
                        else
                        {
                            var bufVal = buffer[bufferIndex];

                            if (bufVal != 0)
                            {
                                while (bufVal > 0)
                                {
                                    int pix = (imageHeight - y - 1) * roundedImageWidth + bufCounter;
                                    imageBuffer[pix] = 1;
                                    bufCounter++;
                                    bufVal--;
                                }
                            }
                            else
                            {
                                dl = true;
                            }
                        }
                    }
                }

                Console.WriteLine ();
            }
        }
    }
}

class BmpHeader
{
    // Sequential layout
    public readonly char[] Signature = { 'B', 'M' };
    public Int32 FileSize;
    public Int32 ReservedField;
    public Int32 PixelArrayOffset;
    public readonly Int32 DibHeaderSize = 0x28;
    public Int32 ImageWidth;
    public Int32 ImageHeight;
    public short Planes;
    public short BitsPerPixel;
    public Int32 Compression;
    public Int32 ImageSize;
    public Int32 PixelsPerMeterX;
    public Int32 PixelsPerMeterY;
    public Int32 ColorsInColorTable;
    public Int32 ImportantColorCount;

    /* ---------------------------------------------------------------------------------------------------------------------------------- */
    public void Write (Stream sinkStream)
    {
        BinaryWriter writer = new BinaryWriter (sinkStream);

        // File header
        writer.Write (Signature);
        writer.Write (FileSize);
        writer.Write (ReservedField);
        writer.Write (PixelArrayOffset);

        // Info header
        writer.Write (DibHeaderSize);
        writer.Write (ImageWidth);
        writer.Write (ImageHeight);
        writer.Write (Planes);
        writer.Write (BitsPerPixel);
        writer.Write (Compression);
        writer.Write (ImageSize);
        writer.Write (PixelsPerMeterX);
        writer.Write (PixelsPerMeterY);
        writer.Write (ColorsInColorTable);
        writer.Write (ImportantColorCount);
    }
}