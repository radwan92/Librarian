using System.IO;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Librarian
{
    class Decompressor
    {
        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public void DecFileTest (string wdtPath)
        {
            var book = new Book (wdtPath);

            var outPath = Path.GetDirectoryName (wdtPath);

            for (int i = 0; i < book.TableOfContents.Count; i++)
                CreateFile (book, book.TableOfContents[i], outPath);

            //CreateFile (book, book.TableOfContents[1], outPath);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void CreateFile (Book book, TzarFileInfo tzarFile, string outputDir)
        {
            Console.WriteLine (string.Format ("File: {0}", tzarFile.Path));
            var fileBytes = Lzss.GetFile (tzarFile, book);

            string fileDir = Path.GetDirectoryName (Path.Combine (outputDir, tzarFile.Path));
            Directory.CreateDirectory (fileDir);
            File.WriteAllBytes (Path.Combine (outputDir, tzarFile.Path), fileBytes);
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

                byte[] contentDecompressed = new byte[book.PageSize];
                var memStream = new MemoryStream (contentDecompressed);
                int decompressedSize = Lzss.Decompress (book, chapter, memStream);

                Console.WriteLine (string.Format ("Chapter: {0} Start: {3:X} End: {4:X} Size: {1} Bytes: {2}", i, chapter.Size, decompressedSize, chapter.StartPosition, chapter.EndPosition));

                decompressedSize = Math.Min (book.PageSize, decompressedSize);

                // TEMP: Byte-by-byte comparison of the original decompressed file against
                // our decompressed file - just to be 100% sure that everything went right
                for (int x = 0; x < decompressedSize; x++)
                {
                    if (comparisonBuffer[x] != contentDecompressed[x])
                    {
                        int misPos = book.PageSize * i + x;
                        DebugUtils.PrintHex (misPos, 0, "Byte mis at");
                        DebugUtils.PrintHex (comparisonBuffer[i], 0, "Original");
                        DebugUtils.PrintHex (contentDecompressed[i], 0, "Ours");
                    }
                }

                outFile.Write (contentDecompressed, 0, decompressedSize);
                outFile.Flush ();
            }

            // TEMP: Simple md5 comparison of the original file against our decompressed
            // file. Clean up / move to a method / remove completely
            using (var md5 = MD5.Create())
            {
                outFile.Seek (0, SeekOrigin.Begin);
                compareFile.Seek (0, SeekOrigin.Begin);

                var ours = md5.ComputeHash (outFile);
                var theirs = md5.ComputeHash (compareFile);

                var ourSum = ToHex (ours, true);
                var theirSum = ToHex (theirs, true);

                Console.WriteLine ();
                Console.WriteLine ("OUR SUM:   " + ourSum);
                Console.WriteLine ("THEIR SUM: " + theirSum);
            }

            outFile.Dispose ();
            inFile.Dispose ();
            compareFile.Dispose ();
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static string ToHex(byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length*2);

            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

            return result.ToString();
        }
    }
}