using Librarian.Rle;
using Librarian.Utils;
using System;
using System.IO;
using System.Linq;

namespace Librarian
{
    public class RleToBmpTester
    {
        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static bool TestRleConversionAgainstReferenceBmps (string rlesDirectory, string referenceBmpsDirectory, bool logInfo = true)
        {
            var rleFiles          = Directory.GetFiles (rlesDirectory, "*.rle", SearchOption.AllDirectories);
            var referenceBmpFiles = Directory.GetFiles (referenceBmpsDirectory, "*.bmp", SearchOption.AllDirectories);

            foreach (var rlePath in rleFiles)
            {
                RleFile rle = RleFile.CreateFromFile (rlePath);
                BmpFile bmp = RleToBmp.Convert (rle);

                var convertedBmpPath = Path.ChangeExtension (rlePath, "bmp");

                bmp.WriteToFile (convertedBmpPath);

                var referenceBmpPath = referenceBmpFiles.SingleOrDefault (b => Path.GetFileNameWithoutExtension (b) == Path.GetFileNameWithoutExtension (rlePath));

                if (referenceBmpPath == null)
                {
                    if (logInfo)
                        Console.WriteLine ("ERROR. Ref bmp not found for " + rle);

                    continue;
                }

                bool areMd5Equal = ValidityUtils.AreMD5Equal (convertedBmpPath, referenceBmpPath);

                if (!areMd5Equal)
                {
                    if (logInfo)
                        Console.WriteLine (string.Format ("Converted BMP differs from the reference BMP: \nConverted: {0} \nReference: {1}", convertedBmpPath, referenceBmpPath));

                    return false;
                }

                File.Delete (convertedBmpPath);
            }

            return true;
        }
    }
}
