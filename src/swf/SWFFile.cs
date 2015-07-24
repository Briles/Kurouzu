using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

// NOTE: To read compressed SWF files, you'll need to download SharpZipLib and add a reference to ICSharpCode.SharpZipLib.dll
//       to this project. SharpZipLib can be downloaded from: http://www.icsharpcode.net/OpenSource/SharpZipLib/

namespace FlashTools
{
    public class SWFFile
    {
        private Stream stream = null;
        private SWFReader swf = null;
        private string fileName = String.Empty;
        private string signature = String.Empty;
        private byte version = 0;
        private uint fileLength = 0;
        private int frameWidth = 0;
        private int frameHeight = 0;
        private Single frameRate = 0.0F;
        private ushort frameCount = 0;
        public List<DefineBitsLossless2> pngImages = new List<DefineBitsLossless2>();

        #region Properties

        public string FileName
        {
            get { return fileName; }
        }

        public string Signature
        {
            get { return signature; }
        }

        public byte Version
        {
            get { return version; }
        }

        public uint FileLength
        {
            get { return fileLength; }
        }

        public int FrameWidth
        {
            get { return frameWidth; }
        }

        public int FrameHeight
        {
            get { return frameHeight; }
        }

        public Single FrameRate
        {
            get { return frameRate; }
        }

        public ushort FrameCount
        {
            get { return frameCount; }
        }

        public List<DefineBitsLossless2> PNGImages
        {
            get { return pngImages; }
        }

        #endregion

        #region Constructors

        public SWFFile(string fileName)
        {
            this.fileName = fileName;
            Trace.WriteLine(String.Format("File        : {0}", this.FileName));

            this.stream = new FileStream(this.fileName, FileMode.Open, FileAccess.Read);
            this.swf = new SWFReader(this.stream);

            if (ReadHeader())
            {
                // Just identify the tag types
                // ** This would normally be the place to start processing tags **
                IdentifyTags();
            }
        }

        #endregion

        private void AddPNG(DefineBitsLossless2 image)
        {
            this.PNGImages.Add(image);
        }

        private void AddSymbol(Dictionary<ulong, string> symbollist)
        {
            foreach (var symbol in symbollist) {
                DefineBitsLossless2 image = this.pngImages.FirstOrDefault(x => x.CharacterID == symbol.Key);
                if (image != null) image.SymbolName = symbol.Value;
            }
        }

        private bool ReadHeader()
        {
            // Read file signature
            this.signature = Encoding.ASCII.GetString(this.swf.ReadUI8(3));     // "FWS" or "CWS" for ZLIB compressed (v6.0 or later)
            if (this.signature != "FWS" &&
                this.signature != "CWS")
            {
                Trace.WriteLine("Not a valid SWF (Flash) file signature");
                return false;
            }
            Trace.WriteLine(String.Format("Signature   : {0}", this.Signature));

            // Read file version
            this.version = this.swf.ReadUI8();
            Trace.WriteLine(String.Format("Version     : {0}", this.Version));

            // File length
            this.fileLength = this.swf.ReadUI32();
            Trace.WriteLine(String.Format("File length : {0} bytes", this.FileLength));

            // If the file is compressed, this is where the ZLIB decompression ("inflate") begins
            if (this.signature == "CWS")
            {
                // Begin inflating stream
                swf.Stream.ReadByte();
                swf.Stream.ReadByte();

                DeflateStream inflatedStream = new DeflateStream(this.stream, CompressionMode.Decompress);
                swf.Stream = inflatedStream;
            }

            // Frame size
            Rect frameSize = new Rect(swf);
            this.frameWidth = frameSize.XMax;
            this.frameHeight = frameSize.YMax;
            Trace.WriteLine(String.Format("Frame width : {0} twips ({1} pixels)", this.FrameWidth, this.FrameWidth / 20));
            Trace.WriteLine(String.Format("Frame height: {0} twips ({1} pixels)", this.FrameHeight, this.FrameHeight / 20));

            // Frame rate (stored in UI8.UI8 format)
            ushort frameRateMinor = swf.ReadUI8();
            ushort frameRateMajor = swf.ReadUI8();
            this.frameRate = Convert.ToSingle(String.Format("{0}.{1}", frameRateMajor, frameRateMinor));    // TODO: Improve this later
            Trace.WriteLine(String.Format("Frame rate  : {0} fps", this.FrameRate));

            // Frame count
            this.frameCount = swf.ReadUI16();
            Trace.WriteLine(String.Format("Frame count : {0}", this.FrameCount));

            return true;
        }

        // Doesn't do much but iterate through the tags
        private void IdentifyTags()
        {
            Tag tag = null;

            do
            {
                tag = new Tag(swf);
                if (tag.Code == 36) AddPNG(tag.Png);
                if (tag.Code == 76) AddSymbol(tag.Symbols);
            } while (tag.Code != 0);
        }

        public void Close()
        {
            this.stream.Close();
        }
    }
}
