namespace Librarian.Wdt
{
    public struct PackTzarFile
    {
        public readonly int      NameLength;
        public readonly string   Path;
        public readonly int      Size;
        public readonly int      Offset;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public PackTzarFile (string path, int nameLength, int size, int offset)
        {
            Path            = path;
            NameLength      = nameLength;
            Size            = size;
            Offset          = offset;
        }
    }
}
