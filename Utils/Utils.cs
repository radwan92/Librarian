﻿using System.Text;

namespace Librarian.Utils
{
    static class Utils
    {
        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static string ToHex(byte[] bytes, bool upperCase = false)
        {
            StringBuilder result = new StringBuilder(bytes.Length*2);

            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

            return result.ToString();
        }
    }
}
