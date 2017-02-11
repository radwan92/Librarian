using System.IO;
using System;

namespace Librarian
{
    class Decompressor
    {
        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void DecFileTest (string wdtPath)
        {
            var book = new Book (wdtPath);

            Lzss.GetFile (book.TableOfContents[1], book);
        }

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
                var chapter = book.ChapterList[i];

                int byteCount;
                var contentDecompressed = Lzss.Decompress (book, chapter, out byteCount);

                Console.WriteLine (string.Format ("Chapter: {0} Start: {3:X} End: {4:X} Size: {1} Bytes: {2}", i, chapter.Size, byteCount, chapter.StartPosition, chapter.EndPosition));

                byteCount = Math.Min (book.PageSize, byteCount);

                for (int x = 0; x < byteCount; x++)
                {
                    if (comparisonBuffer[x] != contentDecompressed[x])
                    {
                        int misPos = book.PageSize * i + x;
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