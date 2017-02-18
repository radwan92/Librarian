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
    }
}
