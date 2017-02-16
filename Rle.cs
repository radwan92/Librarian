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

                int extWidth = imageWidth;
                extWidth += 3;
                extWidth = extWidth >> 2;
                extWidth *= 2;
                extWidth *= 2;

                int imageSize = extWidth * imageHeight;
                byte[] imageBuffer = new byte[imageSize];

                int tableSize = imageHeight * 4;

                byte[] table = new byte[tableSize + 4];

                rleStream.Read (table, 0, tableSize);

                uint arg1 = BitConverter.ToUInt16 (rleHeader, 0x1A);
                uint arg2 = BitConverter.ToUInt16 (rleHeader, 0x1C);

                arg2 = arg2 << 0x10;
                arg2 |= arg1;

                Array.Copy (BitConverter.GetBytes (arg2), 0, table, tableSize, 2);

                bool dl = false;
                for (int y = 0; y < imageHeight; y++)
                {
                    short valueDif = table[y * 4 + 4];
                    valueDif -= table[y * 4];

                    byte[] buffer = new byte[valueDif];
                    rleStream.Read (buffer, 0, valueDif);

                    dl = true;

                    for (int x = 0; x < buffer[0]; x++)
                    {
                        int pix = (imageHeight - y - 1) * extWidth + y;

                        imageBuffer[pix] = 0;
                    }
                }
            }
        }
    }
}
