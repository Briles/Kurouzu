using System.Collections.Generic;

namespace Kurouzu.SWF
{
    public class DefineBitsLossless2
    {
        private byte _bitmapformat;

        #region Properties

        public ushort CharacterId { get; set; }

        public byte BitmapFormat
        {
            get
            {
                return (byte) (_bitmapformat == 5 ? 32 : 16);
            }
            set { _bitmapformat = value; }
        }

        public ushort BitmapWidth { get; set; }

        public ushort BitmapHeight { get; set; }

        public int BitmapArea => BitmapWidth * BitmapHeight;

        public int BitmapStride => BitmapFormat / 8 * BitmapWidth;

        public List<byte> BitmapPixelData { get; set; } = new List<byte>();

        public string SymbolName { get; set; }

        #endregion
    }
}
