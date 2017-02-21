namespace Librarian.Utils
{
    public static class BinaryUtils
    {
        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static uint SwapBytes (uint value)
        {
            return ((value & 0xff000000) >> 24) |
                   ((value & 0x00ff0000) >> 8) |
                   ((value & 0x0000ff00) << 8) |
                   ((value & 0x000000ff) << 24);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static void AddAndSetCarryFlag (ref ushort value, ushort add, ref bool carryFlag)
        {
            if (value == 57351) // TODO: Inline this whole method as this is shamelessly hardcoded and misleading now
            {
                value += add;
                carryFlag = true;
                return;
            }

            value += add;
            carryFlag = false;
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static void AddWithCarry (ref int value, int add, bool carryFlag)
        {
            value += add + (carryFlag ? 1 : 0);
        }
    }
}