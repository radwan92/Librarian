namespace Librarian
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
            try
            {
                checked { value += add; }
            }
            catch
            {
                value += add;
                carryFlag = true;
                return;
            }

            carryFlag = false;
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static void AddWithCarry (ref int value, int add, bool carryFlag)
        {
            value += add + (carryFlag ? 1 : 0);
        }
    }
}