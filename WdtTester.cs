using System.IO;
using System;
using Librarian.Utils;
using Librarian.Wdt;

namespace Librarian
{
    class WdtTester
    {
        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void DecompressWdtToDirectory (string wdtPath, string outputDirectory)
        {
            var wdtFile = WdtFile.CreateFromFile (wdtPath);

            for (int i = 0; i < wdtFile.Contents.Count; i++)
                WriteTzarFileToDisk (wdtFile, wdtFile.Contents[i], outputDirectory);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void WriteTzarFileToDisk (WdtFile wdtFile, TzarFile tzarFile, string outputDirectory)
        {
            var tzarFileBytes = WdtDecompressor.DecompressTzarFile (tzarFile, wdtFile);

            string fileDir = Path.GetDirectoryName (Path.Combine (outputDirectory, tzarFile.Path));
            Directory.CreateDirectory (fileDir);
            File.WriteAllBytes (Path.Combine (outputDirectory, tzarFile.Path), tzarFileBytes);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public bool TestWdtDecompressionAgainstReferenceWdt (string wdtPath, string referenceWdtPath, bool logInfo = true)
        {
            bool isWdtValid = true;

            var     wdtFile             = WdtFile.CreateFromFile (wdtPath);
            string  decompressedWdtPath = Path.ChangeExtension (wdtPath, "temp");

            using (var inFile      = File.OpenRead (wdtPath))
            using (var compareFile = File.OpenRead (referenceWdtPath))
            using (var outFile     = File.Create (decompressedWdtPath))
            {

                byte[] comparisonBuffer = new byte[wdtFile.PageSize];

                for (int i = 0; i < wdtFile.ChapterList.Count; i++)
                {
                    compareFile.Read (comparisonBuffer, 0, comparisonBuffer.Length);
                    var chapter = wdtFile.ChapterList[i];

                    byte[]  contentDecompressed = new byte[wdtFile.PageSize];
                    var     memStream           = new MemoryStream (contentDecompressed);
                    int     decompressedSize    = WdtDecompressor.DecompressChapter (wdtFile, chapter, memStream);

                    if (logInfo)
                    {
                        Console.WriteLine (string.Format ("Chapter: {0} Start: {3:X} End: {4:X} Size: {1} Bytes: {2}",
                            i, chapter.Size, decompressedSize, chapter.StartPosition, chapter.EndPosition));
                    }

                    decompressedSize = Math.Min (wdtFile.PageSize, decompressedSize);

                    // Byte-by-byte comparison of the original decompressed file against
                    // our decompressed file - just to be 100% sure that everything went right
                    for (int x = 0; x < decompressedSize; x++)
                    {
                        if (comparisonBuffer[x] != contentDecompressed[x])
                        {
                            isWdtValid = false;

                            if (logInfo)
                            {
                                int misPos = wdtFile.PageSize * i + x;
                                Console.WriteLine ();
                                DebugUtils.PrintHex (misPos, "Byte mismatch at");
                                DebugUtils.PrintHex (comparisonBuffer[i], "Original");
                                DebugUtils.PrintHex (contentDecompressed[i], "Ours");
                            }
                        }
                    }

                    outFile.Write (contentDecompressed, 0, decompressedSize);
                }
            }

            bool areMd5Equal = ValidityUtils.AreMD5Equal (decompressedWdtPath, referenceWdtPath);
            isWdtValid &= areMd5Equal;

            if (logInfo)
                Console.WriteLine ("Are MD5 equal: " + areMd5Equal);

            File.Delete (decompressedWdtPath);

            return isWdtValid;
        }
    }
}