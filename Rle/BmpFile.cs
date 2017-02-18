using System;
using System.IO;

namespace Librarian.Rle
{
    class BmpFile
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
        public byte[] ColorTable;
        public byte[] ImageBuffer;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public BmpFile (RleFile rle, byte[] imageBuffer)
        {
            ReservedField       = 0;
            ImageWidth          = rle.ImageWidth;
            ImageHeight         = rle.ImageHeight;
            Planes              = 1;
            BitsPerPixel        = (short)(rle.BytesPerPixel * 8);
            Compression         = 0;
            ImageSize           = rle.ImageSize;
            PixelsPerMeterX     = 0;
            PixelsPerMeterY     = 0;
            ColorsInColorTable  = rle.ColorsInColorTable;
            ImportantColorCount = rle.ColorsInColorTable;

            ColorTable  = new byte[ColorsInColorTable * 4];
            ImageBuffer = imageBuffer;

            PixelArrayOffset = ColorTable.Length + 0x36;    // 0x36 = size of the header without color table
            FileSize         = PixelArrayOffset + ImageSize;
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void WriteToFile (string filePath)
        {
            using (var fileStream = File.OpenWrite (filePath))
            {
                BinaryWriter writer = new BinaryWriter (fileStream);

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

                // Color table
                writer.Write (ColorTable);

                // Image
                writer.Write (ImageBuffer);
            }
        }
    }
}
