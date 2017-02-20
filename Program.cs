using System;
using System.IO;

namespace Librarian
{
    class Program
    {
        public static readonly string RLE_EXTENSION = "RLE";
        public static readonly string WDT_EXTENSION = "WDT";

        static readonly string s_wdtPathFileName       = "wdtPath.txt";
        static readonly string s_rlePathFileName       = "rlePath.txt";
        static readonly string s_rleTesterPathFileName = "refBmpPath.txt";

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

            string projPath;
            string refProjPath;
            if (TryGetFilePathFromTextFile (s_rlePathFileName, RLE_EXTENSION, out projPath) && TryGetFilePathFromTextFile (s_rleTesterPathFileName, "REF", out refProjPath))
            {
                Rle.RleDecompressor.DecompressDirWithTest (projPath, refProjPath);
                //Rle.RleDecompressor.Decompress (projPath);
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

            string baseDirectory = Path.GetDirectoryName (System.Reflection.Assembly.GetEntryAssembly ().Location);
            string textFilePath  = Path.Combine (baseDirectory, textFileName);
            string retrievedPath = null;

            if (!File.Exists (textFilePath))
            {
                Console.WriteLine (string.Format ("Text file {0} was not found in: {1}", textFileName, baseDirectory));
                Console.WriteLine ();
                Console.WriteLine (string.Format ("Please create file {0} in the executable directory.\nNext, put the {1}'s path in the first line of the created file", textFileName, fileExtension));
                return false;
            }

            try
            {
                retrievedPath = File.ReadAllLines (textFilePath)[0];
            }
            catch (Exception exception)
            {
                Console.WriteLine (string.Format ("Failed to retrieve {0} path from {1} due to an error: {2}", fileExtension, textFileName, exception.Message));
                return false;
            }

            if (!File.Exists (retrievedPath) && !Directory.Exists (retrievedPath))
            {
                Console.WriteLine (string.Format ("{0} file at: \n{1}\n specified in: \n{2}\n does not exist.", fileExtension, retrievedPath, textFilePath));
                return false;
            }

            path = retrievedPath;
            return true;
        }
    }
}