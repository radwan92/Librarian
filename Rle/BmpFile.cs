using System;
using System.IO;

namespace Librarian.Rle
{
    public class BmpFile
    {
        public static readonly int HEADER_SIZE = 0x36;  // Size of the header without color table

        // Sequential layout
        public readonly char[]  Signature = { 'B', 'M' };
        public readonly Int32   FileSize;
        public readonly Int32   ReservedField;
        public readonly Int32   PixelArrayOffset;
        public readonly Int32   DibHeaderSize = 0x28;
        public readonly Int32   ImageWidth;
        public readonly Int32   ImageHeight;
        public readonly short   Planes;
        public readonly short   BitsPerPixel;
        public readonly Int32   Compression;
        public readonly Int32   ImageSize;
        public readonly Int32   PixelsPerMeterX;
        public readonly Int32   PixelsPerMeterY;
        public readonly Int32   ColorsInColorTable;
        public readonly Int32   ImportantColorCount;
        public readonly byte[]  ColorTable;
        public readonly byte[]  ImageBuffer;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static BmpFile CreateFromRle (RleFile rle, byte[] imageBuffer)
        {
            return new BmpFile (rle, imageBuffer);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        BmpFile (RleFile rle, byte[] imageBuffer)
        {
            ReservedField       = 0;
            ImageWidth          = rle.ImageWidth;
            ImageHeight         = rle.ImageHeight;
            Planes              = 1;
            BitsPerPixel        = (short)(rle.BytesPerPixel * 8);
            Compression         = 0;
            ImageSize           = imageBuffer.Length;
            PixelsPerMeterX     = 0;
            PixelsPerMeterY     = 0;
            ColorsInColorTable  = rle.ColorsInColorTable;
            ImportantColorCount = rle.ColorsInColorTable;
            ColorTable          = rle.ColorTable;
            ImageBuffer         = imageBuffer;

            PixelArrayOffset = ColorTable.Length + HEADER_SIZE;
            FileSize         = PixelArrayOffset + ImageSize;
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void WriteToFile (string filePath)
        {
            using (var fileStream = new FileStream (filePath, FileMode.Create, FileAccess.Write))
            {
                WriteToStream (fileStream);
            }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void WriteToStream (Stream stream)
        {
            BinaryWriter writer = new BinaryWriter (stream);

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
