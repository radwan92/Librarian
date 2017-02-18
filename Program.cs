using System;
using System.IO;

namespace Librarian
{
    class Program
    {
        public static readonly string RLE_EXTENSION = "RLE";
        public static readonly string WDT_EXTENSION = "WDT";
        static readonly string s_wdtPathFileName = "wdtPath.txt";
        static readonly string s_rlePathFileName = "rlePath.txt";

        /* ================================================================================================================================== */
        // ENTRY POINT
        /* ================================================================================================================================== */
        // TODO: Passing wdt path as argument?
        static void Main (string[] args)
        {
            string wdtPath;
            if (TryGetFilePathFromTextFile (s_wdtPathFileName, WDT_EXTENSION, out wdtPath))
            {
                //Decompressor d = new Decompressor ();
                //d.DecompressWdtTest (wdtPath);
                //d.DecFileTest (wdtPath);
            }

            string rlePath;
            if (TryGetFilePathFromTextFile (s_rlePathFileName, RLE_EXTENSION, out rlePath))
            {
                Rle.RleDecompressor.Decompress (rlePath);
            }

            Console.WriteLine ();
            Console.WriteLine ("Press any key to continue...");
            Console.ReadKey ();
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        // TODO: Proper path resolving
        static bool TryGetFilePathFromTextFile (string textFileName, string fileExtension, out string path)
        {
            path = null;

            string baseDirectory    = Path.GetDirectoryName (System.Reflection.Assembly.GetEntryAssembly ().Location);
            string wdtPathFile      = Path.Combine (baseDirectory, textFileName);
            string wdtPath          = null;

            if (!File.Exists (wdtPathFile))
            {
                Console.WriteLine (string.Format ("Text file {0} was not found in: {1}", textFileName, baseDirectory));
                Console.WriteLine ();
                Console.WriteLine (string.Format ("Please create file {0} in the executable directory.\nNext, put the {1}'s path in the first line of the created file", textFileName, fileExtension));
                return false;
            }

            try
            {
                wdtPath = File.ReadAllLines (wdtPathFile)[0];
            }
            catch (Exception exception)
            {
                Console.WriteLine (string.Format ("Failed to retrieve {0} path from {1} due to an error: {2}", fileExtension, textFileName, exception.Message));
                return false;
            }

            if (!File.Exists (wdtPath))
            {
                Console.WriteLine (string.Format ("{0} file at: \n{1}\n specified in: \n{2}\n does not exist.", fileExtension, wdtPath, wdtPathFile));
                return false;
            }

            path = wdtPath;
            return true;
        }
    }
}