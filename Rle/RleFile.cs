using System;
using System.IO;

namespace Librarian.Rle
{
    class RleFile
    {
        public static readonly int HEADER_SIZE = 0x26;

        // Sequential layout
        public byte     Unk_1;              // ESP + 84     // Always 32
        public char[]   EncodingReversed;   // ESP + 85
        public Int32    BatchCount;         // ESP + 88
        public Int32    SpritesPerBatch;    // ESP + 8C
        public Int32    Unk_2;              // ESP + 90     // Always 0
        public char[]   Encoding;           // ESP + 94
        public Int16    ImageWidth;         // ESP + 98
        public Int16    ImageHeight;        // ESP + 9A
        public byte     ImageType;          // ESP + 9C
        public byte     Unk_3;              // ESP + 9C     // Always 0
        public UInt16   TableLength_1;      // ESP + 9E
        public UInt16   TableLength_2;      // ESP + A0
        public Int32    ColorsInColorTable; // ESP + A2
        public Int32    ColorRef;           // ESP + A6     // This is used to do some floating p. calculations at the beginning

        public int      BytesPerPixel;
        public int      BmpImageWidth;  // This shouldn't actually be here as it doesn't belong to the RLE, but oh well...
        public int      ImageSize;
        public int      RefTableSize;
        public byte[]   RefTable;
        public byte[]   RepsTable;
        public byte[]   ColorTable;
        public byte[]   ColorRefBytes;

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public RleFile (string rlePath)
        {
            using (var rleStream = File.OpenRead (rlePath))
            {
                ParseHeader (rleStream);
                SetupForImageType ();
                CalculateImageDimensions ();
                PrepareColorTable (rleStream);
                PrepareRefTable (rleStream);
                PrepareRepsTable (rleStream);
            }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void SetupForImageType ()
        {
            switch (ImageType)
            {
                case 1:
                {
                    BytesPerPixel = 1;
                    BmpImageWidth = ((ImageWidth + 3) / 4) * 4;     // BMP row size has to be a multiple of 4 Bytes, hence rounding to 4 bytes
                }
                break;

                case 3:
                {
                    ColorsInColorTable = 0;
                    BytesPerPixel      = 2;
                    BmpImageWidth = ((ImageWidth + 1) / 2) * 2;
                }
                break;

                case 7:
                {
                    ColorsInColorTable = 2;
                    BytesPerPixel      = 1;
                    BmpImageWidth      = ((ImageWidth + 3) / 4) * 4;     // BMP row size has to be a multiple of 4 Bytes, hence rounding to 4 bytes
                }
                break;

                default:
                    break;
            }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public byte[] GetRepsBuffer (int refTableIndex, int repsTablePosition)
        {
            var repsBufferSize = (UInt16)(BitConverter.ToUInt16 (RefTable, refTableIndex * 4 + 4) - BitConverter.ToUInt16 (RefTable, refTableIndex * 4));
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

            Array.Copy (BitConverter.GetBytes (tableEnd), 0, RefTable, RefTableSize, 4);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void PrepareRepsTable (FileStream rleStream)
        {
            int repsTableSize = (int)(rleStream.Length - rleStream.Position);
            RepsTable = new byte[repsTableSize];

            rleStream.Read (RepsTable, 0, repsTableSize);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void PrepareColorTable (FileStream rleStream)
        {
            ColorTable = new byte[ColorsInColorTable * 4];

            if (ImageType == 1)
            {
                rleStream.Read (ColorTable, 0, ColorTable.Length);
            }
            else if (ImageType == 7)
            {
                double colorCoefficient = BitConverter.ToSingle (new byte[] { 0xE7, 0x9C, 0x03, 0x41 }, 0);

                float c1 = (ColorRef >> 0xA) & 0x1F;
                float c2 = (ColorRef >> 0x5) & 0x1F;
                float c3 = ((byte)ColorRef) & 0x1F;

                ColorTable[0] = (byte)((int) (c1 * colorCoefficient));
                ColorTable[1] = (byte)((int) (c2 * colorCoefficient));
                ColorTable[2] = (byte)((int) (c3 * colorCoefficient));
            }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void ParseHeader (FileStream rleStream)
        {
            byte[] rleHeader = new byte[HEADER_SIZE];
            rleStream.Read (rleHeader, 0, HEADER_SIZE);

            Unk_1                 = rleHeader[0];
            EncodingReversed      = System.Text.Encoding.ASCII.GetChars (rleHeader, 0x1, 0x3);
            BatchCount            = BitConverter.ToInt32 (rleHeader, 0x4);
            SpritesPerBatch       = BitConverter.ToInt32 (rleHeader, 0x8);
            Unk_2                 = BitConverter.ToInt32 (rleHeader, 0xC);
            Encoding              = System.Text.Encoding.ASCII.GetChars (rleHeader, 0x10, 0x4);
            ImageWidth            = BitConverter.ToInt16 (rleHeader, 0x14);
            ImageHeight           = BitConverter.ToInt16 (rleHeader, 0x16);
            ImageType             = rleHeader[0x18];
            Unk_3                 = rleHeader[0x19];
            TableLength_1         = BitConverter.ToUInt16 (rleHeader, 0x1A);
            TableLength_2         = BitConverter.ToUInt16 (rleHeader, 0x1C);
            ColorsInColorTable    = BitConverter.ToInt32 (rleHeader, 0x1E);
            ColorRef              = BitConverter.ToInt32 (rleHeader, 0x22);
            ColorRefBytes         = BitConverter.GetBytes (ColorRef);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void CalculateImageDimensions ()
        {
            ImageSize = ImageHeight * BmpImageWidth * BytesPerPixel;
        }
    }
}
