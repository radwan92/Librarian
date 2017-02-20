using System.Text;

namespace Librarian
{
    static class Utils
    {
        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        // TODO: Transform into switch (or evaluation if possible)
        static readonly byte[] m_bitLengthsTable =
        {
            0x00, 0x01, 0x02, 0x03,     0x04, 0x0F, 0x0F, 0x0F,     0x0F, 0x0F, 0x0F, 0x0F,     0x0F, 0x0F, 0x0F, 0x0F,
            0x05, 0x06, 0x07, 0x08,     0x09, 0x0F, 0x0F, 0x0F,     0x0F, 0x0F, 0x0F, 0x0F,     0x0F, 0x0F, 0x0F, 0x0F,
            0x0A, 0x0B, 0x0C, 0x0D,     0x0E
        };

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static int GetBitLenghts (int coefficient)
        {
            return m_bitLengthsTable [coefficient - 0xA1];
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
