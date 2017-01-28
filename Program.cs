using System;
using System.IO;

namespace Librarian
{
    class Program
    {
        static readonly string s_wdtPathFileName = "wdtPath.txt";

        /* ================================================================================================================================== */
        // ENTRY POINT
        /* ================================================================================================================================== */
        // TODO: Passing wdt path as argument?
        static void Main (string[] args)
        {
            string wdtPath;
            if (TryGetWdtPath (out wdtPath))
            {
                Decompressor d = new Decompressor ();
                d.Decompress (wdtPath);
            }

            Console.WriteLine ();
            Console.WriteLine ("Press any key to continue...");
            Console.ReadKey ();
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        // TODO: Proper path resolving
        // I'm currently working on IMAGES.WDT and didn't yet test any other file. Expect anything when loading wdt other than IMAGES
        static bool TryGetWdtPath (out string path)
        {
            path = null;

            string baseDirectory    = Path.GetDirectoryName (System.Reflection.Assembly.GetEntryAssembly ().Location);
            string wdtPathFile      = Path.Combine (baseDirectory, s_wdtPathFileName);
            string wdtPath          = null;

            if (!File.Exists (wdtPathFile))
            {
                Console.WriteLine (string.Format ("Text file {0} was not found in: {1}", s_wdtPathFileName, baseDirectory));
                Console.WriteLine ();
                Console.WriteLine (string.Format ("Please create file {0} in the executable directory.\nNext, put the WDT's path in the first line of the created file", s_wdtPathFileName));
                return false;
            }

            try
            {
                wdtPath = File.ReadAllLines (wdtPathFile)[0];
            }
            catch (Exception exception)
            {
                Console.WriteLine (string.Format ("Failed to retrieve WDT path from {0} due to an error: {1}", s_wdtPathFileName, exception.Message));
                return false;
            }

            if (!File.Exists (wdtPath))
            {
                Console.WriteLine (string.Format ("WDT file at: \n{0}\n specified in: \n{1}\n does not exist.", wdtPath, wdtPathFile));
                return false;
            }

            path = wdtPath;
            return true;
        }
    }
}