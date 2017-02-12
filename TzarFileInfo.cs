namespace Librarian
{
    class TzarFileInfo
    {
        public int      NameLength      { get; set; }
        public string   Path            { get; set; }
        public string   Name            { get; set; }
        public int      Size            { get; set; }
        public int      Offset          { get; set; }

        public TzarFileInfo (string path, int nameLength, int size, int offset)
        {
            Path            = path;
            NameLength      = nameLength;
            Size            = size;
            Offset          = offset;
            Name            = System.IO.Path.GetFileName (Path);
        }
    }
}
