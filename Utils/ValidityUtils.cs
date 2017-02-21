using System.IO;
using System.Security.Cryptography;

namespace Librarian.Utils
{
    public static class ValidityUtils
    {
        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        // TEMP: Simple md5 comparison of the original file against our decompressed
        // file. Clean up / move to a method / remove completely
        public static bool AreMD5Equal (string fileA, string fileB)
        {
            using (var md5 = MD5.Create ())
            using (var aStream = new FileStream (fileA, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var bStream = new FileStream (fileB, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var aSum = md5.ComputeHash (aStream);
                var bSum = md5.ComputeHash (bStream);

                var aSumHex = Utils.ToHex (aSum, true);
                var bSumHex = Utils.ToHex (bSum, true);

                return aSumHex == bSumHex;
            }
        }
    }
}
