using System;
using System.IO;

namespace Librarian.Rle
{
    class RleDecompressor
    {
        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static void Decompress (string rlePath)
        {
            RleFile rle = new RleFile (rlePath);

            byte[] imageBuffer = new byte[rle.ImageSize];

            // ESP + 13  :: 1
            // ESP + 14  :: repValue / ??
            // ESP + 18  :: *Buffer
            // ESP + 1C  :: repsBuffer size
            // ESP + 7C  :: repsBuffer size
            // ESP + 24  :: x
            // ESP + 28  :: pixCounter
            // ESP + 30  :: 1
            // ESP + 38  :: *rleTable
            // ESP + 2C  :: *repsBuffer[repIndex]

            bool    dl                = false;
            int     pixCounter        = 0;
            int     repsTablePosition = 0;

            for (int y = 0; y < rle.ImageHeight; y++)
            {
                byte[] repsBuffer = rle.GetRepsBuffer (y, repsTablePosition);
                repsTablePosition += repsBuffer.Length;

                pixCounter = 0;
                dl         = true;

                for (int repIndex = 0; repIndex < repsBuffer.Length; repIndex++)
                {
                    var repValue = repsBuffer[repIndex];

                    if (dl)
                    {
                        if (repValue != 0)
                        {
                            for (int x = 0; x < repValue; x++)
                            {
                                int pixIndex = (rle.ImageHeight - y - 1) * rle.BmpImageWidth + pixCounter;
                                imageBuffer[pixIndex] = 0;
                                pixCounter++;
                            } 
                        }

                        dl = false;
                        continue;
                    }
                    else
                    {
                        if (repValue != 0)
                        {
                            while (repValue > 0)
                            {
                                int pix = (rle.ImageHeight - y - 1) * rle.BmpImageWidth + pixCounter;
                                imageBuffer[pix] = 1;
                                pixCounter++;
                                repValue--;
                            }
                        }

                        dl = true;
                    }
                }
            }

            string decompressedFilePath = Path.Combine (Path.GetDirectoryName (rlePath), "testFile.bmp");

            var bmpFile = new BmpFile (rle, imageBuffer);
            bmpFile.WriteToFile (decompressedFilePath);
        }
    }
}