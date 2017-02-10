namespace Librarian
{
    class TzarFileInfo
    {
        public int      NameLength      { get; set; }
        public string   Name            { get; set; }
        public int      Length          { get; set; }
        public int      Offset          { get; set; }

        public TzarFileInfo (string name, int nameLength, int length, int offset)
        {
            Name            = name;
            NameLength      = nameLength;
            Length          = length;
            Offset          = offset;
        }
    }
}
