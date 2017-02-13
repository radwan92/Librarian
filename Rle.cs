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

                int counter = 0;
                int ecx = 0;

                short x = table[counter * 4 + 4];
                x -= table[counter * 4];
                byte[] buffer = new byte[x];
                rleStream.Read (buffer, 0, x);

                bool dl = true;

                if (dl)
                {
                    bool isFByteEmpty = buffer[0] == 0;

                    if (isFByteEmpty)
                        goto secondPart;

                    while (ecx < buffer[0])
                    {
                        bool isTypeSeven = rleHeader[0x18] == 7;

                        if (!isTypeSeven)
                            goto skipPart;

                        int tempRow = imageHeight - counter - 1;
                        tempRow *= extWidth;

                        imageBuffer[tempRow] = 0;

                        ecx++;
                        counter++;

                        skipPart:
                            ;
                    }
                }

                secondPart:
                    ;
            }
        }
    }
}
