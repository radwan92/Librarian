using System;
using System.IO;

namespace Librarian.Rle
{
    // TODO: Remove unused variables like Unk_1-3, BatchCount, SpritesPerBatch
    public class RleFile
    {
        public static readonly int HEADER_SIZE = 0x26;

        // Sequential layout
        /* ESP + 84 */ public readonly byte     Unk_1;                  // Always 32
        /* ESP + 85 */ public readonly char[]   EncodingReversed;   
        /* ESP + 88 */ public readonly Int32    BatchCount;         
        /* ESP + 8C */ public readonly Int32    SpritesPerBatch;    
        /* ESP + 90 */ public readonly Int32    Unk_2;                  // Always 0
        /* ESP + 94 */ public readonly char[]   Encoding;           
        /* ESP + 98 */ public readonly Int16    ImageWidth;         
        /* ESP + 9A */ public readonly Int16    ImageHeight;        
        /* ESP + 9C */ public readonly byte     ImageType;          
        /* ESP + 9C */ public readonly byte     Unk_3;                  // Always 0
        /* ESP + 9E */ public readonly UInt16   TableLength_1;      
        /* ESP + A0 */ public readonly UInt16   TableLength_2;      
        /* ESP + A2 */ public Int32             ColorsInColorTable { get; private set; } 
        /* ESP + A6 */ public readonly Int32    ColorRef;               // This is used to do some floating p. calculations at the beginning

        public int      BytesPerPixel   { get; private set; }
        public int      RefTableSize    { get; private set; }
        public byte[]   RefTable        { get; private set; }
        public byte[]   RepsTable       { get; private set; }
        public byte[]   ColorTable      { get; private set; }
        public byte[]   ColorRefBytes   { get; private set; }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static RleFile CreateFromFile (string rlePath)
        {
            using (var rleStream = new FileStream (rlePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return new RleFile (rleStream);
            }
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        public static RleFile CreateFromStream (Stream stream)
        {
            return new RleFile (stream);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        RleFile (Stream rleStream)
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

            SetupForImageType ();
            PrepareColorTable (rleStream);
            PrepareRefTable (rleStream);
            PrepareRepsTable (rleStream);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void SetupForImageType ()
        {
            switch (ImageType)
            {
                case 1:
                {
                    BytesPerPixel = 1;
                }
                break;

                case 3:
                {
                    ColorsInColorTable = 0;
                    BytesPerPixel      = 2;
                }
                break;

                case 7:
                {
                    ColorsInColorTable = 2;
                    BytesPerPixel      = 1;
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
        void PrepareRefTable (Stream rleStream)
        {
            RefTableSize = ImageHeight * 4;
            RefTable     = new byte[RefTableSize + 4];

            rleStream.Read (RefTable, 0, RefTableSize);

            int tableEnd = TableLength_2 << 0x10;
            tableEnd |= TableLength_1;

            Array.Copy (BitConverter.GetBytes (tableEnd), 0, RefTable, RefTableSize, 4);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void PrepareRepsTable (Stream rleStream)
        {
            int repsTableSize = (int)(rleStream.Length - rleStream.Position);
            RepsTable = new byte[repsTableSize];

            rleStream.Read (RepsTable, 0, repsTableSize);
        }

        /* ---------------------------------------------------------------------------------------------------------------------------------- */
        void PrepareColorTable (Stream rleStream)
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
    }
}
