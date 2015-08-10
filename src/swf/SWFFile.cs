using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;

namespace Kurouzu.SWF
{
    public class SwfFile
    {
        private readonly Stream _stream;
        private readonly SwfReader _swf;

        #region Properties

        public uint FileLength { get; } = 0;

        public string FileName { get; }

        public List<DefineBitsLossless2> PngImages { get; } = new List<DefineBitsLossless2>();

        #endregion

        #region Constructors

        public SwfFile(string fileName)
        {
            FileName = Path.GetFileNameWithoutExtension(fileName);
            _stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            _swf = new SwfReader(_stream);

            if (ReadHeader())
            {
                IdentifyTags();
            }

            // Close the stream
            _stream.Close();
        }

        #endregion

        private void AddPng(DefineBitsLossless2 image)
        {
            PngImages.Add(image);
        }

        private void MatchSymbols(Dictionary<short, string> symbols)
        {
            foreach (var symbol in symbols) {
                var png = PngImages.FirstOrDefault(x => x.CharacterId == symbol.Key);
                if (png != null) png.SymbolName = symbol.Value;
            }

        }

        private bool ReadHeader()
        {
            // Signature
            _swf.ReadUI8(3);

            // File version
            _swf.ReadUI8();

            // File length
            _swf.ReadUI32();

            // The swf is Zlib compressed from here on
            _swf.Stream.ReadByte(); // The first two bytes are Zlib info
            _swf.Stream.ReadByte(); //
            DeflateStream inflatedStream = new DeflateStream(_stream, CompressionMode.Decompress);
            _swf.Stream = inflatedStream;

            // Frame size
            int nBits = (int) _swf.ReadUB(5);
            _swf.ReadSB(nBits);
            _swf.ReadSB(nBits);
            _swf.ReadSB(nBits);
            _swf.ReadSB(nBits);

            // Frame rate (stored in UI8.UI8 format)
            _swf.ReadUI8();
            _swf.ReadUI8();

            // Frame count
            _swf.ReadUI16();

            return true;
        }

        // Iterate through the tags
        private void IdentifyTags()
        {
            Tag tag = null;

            do
            {
                tag = new Tag(_swf);
                if (tag.Code == 36) AddPng(tag.Png);
                if (tag.Code == 76) MatchSymbols(tag.Symbols);
            } while (tag.Code != 0);
        }

        // Extract all the PNGs
        public void ExtractImages(string outputPath)
        {
            foreach (DefineBitsLossless2 image in PngImages)
            {
                Console.WriteLine("Extracting {0}", image.SymbolName);

                string destinationPath = Path.Combine(outputPath, string.Format("{0}.png", image.SymbolName));

                byte[] bitmapPixelData = image.BitmapPixelData.ToArray();

                GCHandle pinnedBitmapPixelData = GCHandle.Alloc(bitmapPixelData, GCHandleType.Pinned);
                IntPtr bitmapPixelDataPtr = pinnedBitmapPixelData.AddrOfPinnedObject();

                using (Bitmap newPng = new Bitmap(image.BitmapWidth, image.BitmapHeight, image.BitmapStride, PixelFormat.Format32bppPArgb, bitmapPixelDataPtr))
                {
                    newPng.Save(destinationPath, ImageFormat.Png);
                    newPng.Dispose();
                    pinnedBitmapPixelData.Free();
                }
            }
        }
    }
}
