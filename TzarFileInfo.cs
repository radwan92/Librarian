namespace Librarian
{
    class TzarFileInfo
    {
        public int      NameLength      { get; set; }
        public string   Name            { get; set; }
        public int      Size            { get; set; }
        public int      Offset          { get; set; }

        public TzarFileInfo (string name, int nameLength, int size, int offset)
        {
            Name            = name;
            NameLength      = nameLength;
            Size            = size;
            Offset          = offset;
        }
    }
}
