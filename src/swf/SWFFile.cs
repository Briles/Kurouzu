using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;

namespace SWFTools
{
    public class SWFFile
    {
        private Stream stream = null;
        private SWFReader swf = null;
        private uint fileLength = 0;
        private string fileName = null;
        public List<DefineBitsLossless2> pngImages = new List<DefineBitsLossless2>();

        #region Properties

        public uint FileLength
        {
            get { return fileLength; }
        }

        public string FileName
        {
            get { return fileName; }
        }

        public List<DefineBitsLossless2> PNGImages
        {
            get { return pngImages; }
        }

        #endregion

        #region Constructors

        public SWFFile(string fileName)
        {
            this.fileName = Path.GetFileNameWithoutExtension(fileName);
            stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            swf = new SWFReader(stream);

            if (ReadHeader())
            {
                IdentifyTags();
            }

            // Close the stream
            stream.Close();
        }

        #endregion

        private void AddPNG(DefineBitsLossless2 image)
        {
            PNGImages.Add(image);
        }

        private void MatchSymbols(Dictionary<short, string> symbols)
        {
            foreach (var symbol in symbols) {
                var png = PNGImages.FirstOrDefault(x => x.CharacterID == symbol.Key);
                if (png != null) png.SymbolName = symbol.Value;
            }

        }

        private bool ReadHeader()
        {
            // Signature
            swf.ReadUI8(3);

            // File version
            swf.ReadUI8();

            // File length
            swf.ReadUI32();

            // The swf is Zlib compressed from here on
            swf.Stream.ReadByte(); // The first two bytes are Zlib info
            swf.Stream.ReadByte(); //
            DeflateStream inflatedStream = new DeflateStream(stream, CompressionMode.Decompress);
            swf.Stream = inflatedStream;

            // Frame size
            int nBits = (int) swf.ReadUB(5);
            swf.ReadSB(nBits);
            swf.ReadSB(nBits);
            swf.ReadSB(nBits);
            swf.ReadSB(nBits);

            // Frame rate (stored in UI8.UI8 format)
            swf.ReadUI8();
            swf.ReadUI8();

            // Frame count
            swf.ReadUI16();

            return true;
        }

        // Iterate through the tags
        private void IdentifyTags()
        {
            Tag tag = null;

            do
            {
                tag = new Tag(swf);
                if (tag.Code == 36) AddPNG(tag.Png);
                if (tag.Code == 76) MatchSymbols(tag.Symbols);
            } while (tag.Code != 0);
        }

        // Extract all the PNGs
        public void ExtractImages(string outputPath)
        {
            foreach (DefineBitsLossless2 image in PNGImages)
            {
                Console.WriteLine("Extracting {0}", image.SymbolName);

                string destinationPath = Path.Combine(outputPath, string.Format("{0}.png", image.SymbolName));

                byte[] BitmapPixelData = new byte[image.BitmapArea];
                BitmapPixelData = image.BitmapPixelData.ToArray();

                GCHandle PinnedBitmapPixelData = GCHandle.Alloc(BitmapPixelData, GCHandleType.Pinned);
                IntPtr BitmapPixelDataPtr = PinnedBitmapPixelData.AddrOfPinnedObject();

                using (Bitmap newPNG = new Bitmap(image.BitmapWidth, image.BitmapHeight, image.BitmapStride, PixelFormat.Format32bppPArgb, BitmapPixelDataPtr))
                {
                    newPNG.Save(destinationPath, ImageFormat.Png);
                    newPNG.Dispose();
                    PinnedBitmapPixelData.Free();
                }
            }
        }
    }
}
