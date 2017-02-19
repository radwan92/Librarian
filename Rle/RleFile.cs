using System;
using System.IO;

namespace Librarian.Rle
{
    class RleFile
    {
        public static readonly int HEADER_SIZE = 0x26;

        // Sequential layout
        public byte     UnknownField_1;     // ESP + 84
        public char[]   EncodingReversed;   // ESP + 85
        public Int32    Unk_12;             // ESP + 88
        public Int32    Unk_14;             // ESP + 8C
        public Int32    UnknownField_2;     // ESP + 90
        public char[]   Encoding;           // ESP + 94
        public Int16    ImageWidth;         // ESP + 98
        public Int16    ImageHeight;        // ESP + 9A
        public byte     ImageType;          // ESP + 9C
        public byte     Unk_25;             // ESP + 9C
        public UInt16   TableLength_1;      // ESP + 9E
        public UInt16   TableLength_2;      // ESP + A0
        public Int32    ColorsInColorTable; // ESP + A2
        public Int32    UnknownField_4;     // ESP + A6     // This is used to do some floating p. calculations at the beginning

        public int      BytesPerPixel;
        public int      BmpImageWidth;  // This shouldn't actually be here as it doesn't belong to the RLE, but oh well...
        public int      ImageSize;
        public int      RefTableSize;
        public byte[]   RefTable;
        public byte[]   RepsTable;
        public byte[]   ColorTable;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public RleFile (string rlePath)
        {
            using (var rleStream = File.OpenRead (rlePath))
            {
                ParseHeader (rleStream);
                CalculateImageDimensions ();
                PrepareRefTable (rleStream);
                PrepareRepsTable (rleStream);

                // This is hardcoded for type 7
                ColorsInColorTable = 2;
                BytesPerPixel      = 1;

                PrepareColorTable ();
            }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public byte[] GetRepsBuffer (int refTableIndex, int repsTablePosition)
        {
            var repsBufferSize = BitConverter.ToInt16 (RefTable, refTableIndex * 4 + 4) - BitConverter.ToInt16 (RefTable, refTableIndex * 4);
            var repsBuffer = new byte[repsBufferSize];

            Array.Copy (RepsTable, repsTablePosition, repsBuffer, 0, repsBufferSize);

            return repsBuffer;
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void PrepareRefTable (FileStream rleStream)
        {
            RefTableSize = ImageHeight * 4;
            RefTable     = new byte[RefTableSize + 4];

            rleStream.Read (RefTable, 0, RefTableSize);

            int tableEnd = TableLength_2 << 0x10;
            tableEnd |= TableLength_1;

            Array.Copy (BitConverter.GetBytes (tableEnd), 0, RefTable, RefTableSize, 2);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void PrepareRepsTable (FileStream rleStream)
        {
            int repsTableSize = (int)(rleStream.Length - rleStream.Position);
            RepsTable = new byte[repsTableSize];

            rleStream.Read (RepsTable, 0, repsTableSize);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void PrepareColorTable ()
        {
            ColorTable = new byte[ColorsInColorTable * 4];

            // This is probably for type 7 only...
            double colorCoefficient = BitConverter.ToSingle (new byte[] { 0xE7, 0x9C, 0x03, 0x41 }, 0);

            float c1 = (UnknownField_4 >> 0xA) & 0x1F;
            float c2 = (UnknownField_4 >> 0x5) & 0x1F;
            float c3 = ((byte)UnknownField_4) & 0x1F;

            ColorTable[0] = (byte)((int) (c1 * colorCoefficient));
            ColorTable[1] = (byte)((int) (c2 * colorCoefficient));
            ColorTable[2] = (byte)((int) (c3 * colorCoefficient));
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void ParseHeader (FileStream rleStream)
        {
            byte[] rleHeader = new byte[HEADER_SIZE];
            rleStream.Read (rleHeader, 0, HEADER_SIZE);

            UnknownField_1       = rleHeader[0];
            EncodingReversed     = System.Text.Encoding.ASCII.GetChars (rleHeader, 0x1, 0x3);
            Unk_12               = BitConverter.ToInt32 (rleHeader, 0x4);
            Unk_14               = BitConverter.ToInt32 (rleHeader, 0x8);
            UnknownField_2       = BitConverter.ToInt32 (rleHeader, 0xC);
            Encoding             = System.Text.Encoding.ASCII.GetChars (rleHeader, 0x10, 0x4);
            ImageWidth           = BitConverter.ToInt16 (rleHeader, 0x14);
            ImageHeight          = BitConverter.ToInt16 (rleHeader, 0x16);
            ImageType            = rleHeader[0x18];
            Unk_25               = rleHeader[0x19];
            TableLength_1        = BitConverter.ToUInt16 (rleHeader, 0x1A);
            TableLength_2        = BitConverter.ToUInt16 (rleHeader, 0x1C);
            ColorsInColorTable   = BitConverter.ToInt32 (rleHeader, 0x1E);
            UnknownField_4       = BitConverter.ToInt32 (rleHeader, 0x22);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void CalculateImageDimensions ()
        {
            BmpImageWidth = ((ImageWidth + 3) / 4) * 4;     // BMP row size has to be a multiple of 4 Bytes, hence rounding to 4 bytes
            ImageSize     = ImageHeight * BmpImageWidth;
        }
    }
}
