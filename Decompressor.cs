using System.IO;
using System;

namespace Librarian
{
    class Decompressor
    {
        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void DecompressWdtTest (string filePath)
        {
            // SETUP: Two versions of the WDT file should be located at filePath -
            // decompressed (with *.dcm extensions) and compressed (with *.cmp extensions)
            // Decompressor will produce another decompressed file with extensions *.xxx
            // Provided decompressed version will be used to test decompression correctness

            string comprExt = "cmp";
            string decmpExt = "dcm";

            filePath = Path.ChangeExtension (filePath, comprExt);

            var book = new Book (filePath);

            var inFile      = File.OpenRead (filePath);
            var compareFile = File.OpenRead (Path.ChangeExtension (filePath, decmpExt));
            var outFile     = File.Create (Path.ChangeExtension (filePath, "xxx"));

            byte[] comparisonBuffer = new byte[book.PageSize];

            for (int i = 0; i < book.ChapterList.Count; i++)
            {
                compareFile.Read (comparisonBuffer, 0, comparisonBuffer.Length);

                var contentDecompressed = new byte[book.ChapterBufferSize + 40];  // Additional safety bytes for LZSS

                var chapter = book.ChapterList[i];
                int chapterCopyOffset = (int)(book.ChapterBufferSize - chapter.Size);

                inFile.Seek (chapter.StartPosition, SeekOrigin.Begin);
                inFile.Read (contentDecompressed, chapterCopyOffset, (int)chapter.Size);

                int byteCount;
                LzssDecompressor.Decompress (contentDecompressed, chapter, out byteCount);

                Console.WriteLine (string.Format ("Chapter: {0} Start: {3:X} End: {4:X} Size: {1} Bytes: {2}", i, chapter.Size, byteCount, chapter.StartPosition, chapter.EndPosition));

                byteCount = Math.Min ((int)book.PageSize, byteCount);

                for (int x = 0; x < byteCount; x++)
                {
                    if (comparisonBuffer[x] != contentDecompressed[x])
                    {
                        int misPos = (int)book.PageSize * i + x;
                        DebugUtils.PrintHex (misPos, 0, "Byte mis at");
                        DebugUtils.PrintHex (comparisonBuffer[i], 0, "Original");
                        DebugUtils.PrintHex (contentDecompressed[i], 0, "Ours");
                    }
                }

                outFile.Write (contentDecompressed, 0, byteCount);
                outFile.Flush ();
            }

            outFile.Dispose ();
        }
    }
}