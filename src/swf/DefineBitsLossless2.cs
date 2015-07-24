using System.Collections.Generic;

namespace FlashTools
{
    public class DefineBitsLossless2
    {
        private ushort characterid;
        private byte bitmapformat;
        private ushort bitmapwidth;
        private ushort bitmapheight;
        private List<byte> bitmappixeldata = new List<byte>();
        private string symbolname;

        #region Properties

        public ushort CharacterID
        {
            get { return characterid; }
            set { characterid = value; }
        }

        public byte BitmapFormat
        {
            get {
                if (bitmapformat == 5) return 32;
                else return 16;
            }
            set { bitmapformat = value; }
        }

        public ushort BitmapWidth
        {
            get { return bitmapwidth; }
            set { bitmapwidth = value; }
        }

        public ushort BitmapHeight
        {
            get { return bitmapheight; }
            set { bitmapheight = value; }
        }

        public int BitmapArea
        {
            get { return bitmapwidth * bitmapheight; }
        }

        public int BitmapStride
        {
            get { return BitmapFormat / 8 * BitmapWidth; }
        }

        public List<byte> BitmapPixelData
        {
            get { return bitmappixeldata; }
            set { bitmappixeldata = value; }
        }

        public string SymbolName
        {
            get { return symbolname; }
            set { symbolname = value; }
        }

        #endregion
    }
}
