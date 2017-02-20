using System;
using System.IO;
using System.Linq;

namespace Librarian.Rle
{
    class RleDecompressor
    {
        // ESP + 13  :: BytesPerPixel
        // ESP + 14  :: repValue / ??
        // ESP + 18  :: *Buffer
        // ESP + 1C  :: repsBuffer size
        // ESP + 7C  :: repsBuffer size
        // ESP + 24  :: repIndex
        // ESP + 28  :: pixCounter
        // ESP + 30  :: BytesPerPixel
        // ESP + 38  :: *rleTable
        // ESP + 2C  :: *repsBuffer[repIndex]

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static void DecompressDirWithTest (string directory, string refDirectory)
        {
            var rleFiles = Directory.GetFiles (directory, "*.rle", SearchOption.AllDirectories);
            var bmpFiles = Directory.GetFiles (refDirectory, "*.bmp", SearchOption.AllDirectories);

            foreach (var rle in rleFiles)
            {
                Decompress (rle);

                var bmp    = Path.ChangeExtension (rle, "bmp");
                var refBmp = bmpFiles.SingleOrDefault (b => Path.GetFileNameWithoutExtension (b) == Path.GetFileNameWithoutExtension (rle));

                if (refBmp == null)
                {
                    Console.WriteLine ("ERROR. Ref bmp not found for " + rle);
                    return;
                }

                bool areMd5Equal = ValidityUtils.AreMD5Equal (bmp, refBmp);

                if (!areMd5Equal)
                {
                    Console.WriteLine (rle + " not equal");
                    return;
                }
            }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static void Decompress (string rlePath)
        {
            RleFile rle = new RleFile (rlePath);

            byte[] imageBuffer = new byte[rle.ImageSize];

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
                                if (rle.ImageType == 7)
                                {
                                    int pixIndex = (rle.ImageHeight - y - 1) * rle.BmpImageWidth + pixCounter;
                                    imageBuffer[pixIndex] = 0;
                                }
                                else
                                {
                                    int pixIndex = (rle.ImageHeight - y - 1) * rle.BmpImageWidth + pixCounter;
                                    pixIndex *= rle.BytesPerPixel;

                                    Array.Copy (rle.ColorRefBytes, 0, imageBuffer, pixIndex, rle.BytesPerPixel);
                                }

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
                                if (rle.ImageType == 1)
                                {
                                    int pix = (rle.ImageHeight - y - 1) * rle.BmpImageWidth + pixCounter;
                                    imageBuffer[pix] = repsBuffer[repIndex + 1];
                                    repIndex++;
                                }
                                else if (rle.ImageType == 3)
                                {
                                    int pix = (rle.ImageHeight - y - 1) * rle.BmpImageWidth + pixCounter;
                                    imageBuffer[pix * 2]     = repsBuffer[repIndex + 1];
                                    imageBuffer[pix * 2 + 1] = repsBuffer[repIndex + 2];

                                    repIndex += 2;
                                }
                                else if (rle.ImageType == 7)
                                {
                                    int pix = (rle.ImageHeight - y - 1) * rle.BmpImageWidth + pixCounter;
                                    imageBuffer[pix] = 1;
                                }

                                pixCounter++;
                                repValue--;
                            }
                        }

                        dl = true;
                    }
                }
            }

            string decompressedFilePath = Path.ChangeExtension (rlePath, "bmp");

            var bmpFile = new BmpFile (rle, imageBuffer);
            bmpFile.WriteToFile (decompressedFilePath);
        }
    }
}