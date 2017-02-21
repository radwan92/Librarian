using System;

namespace Librarian.Rle
{
    public static class RleToBmp
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
        public static BmpFile Convert (RleFile rle)
        {
            // BMP row size has to be a multiple of 4 Bytes, hence rounding to 4 bytes
            var bmpImageWidth = rle.ImageType == 3 ? ((rle.ImageWidth + 1) / 2) * 2 : ((rle.ImageWidth + 3) / 4) * 4;
            var imageSize     = rle.ImageHeight * bmpImageWidth * rle.BytesPerPixel;

            byte[] imageBuffer = new byte[imageSize];

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
                                    int pixIndex = (rle.ImageHeight - y - 1) * bmpImageWidth + pixCounter;
                                    imageBuffer[pixIndex] = 0;
                                }
                                else
                                {
                                    int pixIndex = (rle.ImageHeight - y - 1) * bmpImageWidth + pixCounter;
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
                                    int pix = (rle.ImageHeight - y - 1) * bmpImageWidth + pixCounter;
                                    imageBuffer[pix] = repsBuffer[repIndex + 1];
                                    repIndex++;
                                }
                                else if (rle.ImageType == 3)
                                {
                                    int pix = (rle.ImageHeight - y - 1) * bmpImageWidth + pixCounter;
                                    imageBuffer[pix * 2]     = repsBuffer[repIndex + 1];
                                    imageBuffer[pix * 2 + 1] = repsBuffer[repIndex + 2];

                                    repIndex += 2;
                                }
                                else if (rle.ImageType == 7)
                                {
                                    int pix = (rle.ImageHeight - y - 1) * bmpImageWidth + pixCounter;
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

            return BmpFile.CreateFromRle (rle, imageBuffer);
        }
    }
}