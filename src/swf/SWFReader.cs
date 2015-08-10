using System;
using System.IO;
using System.Text;

namespace Kurouzu.SWF
{
    /// <summary>
    ///     Class that makes it easier to read SWF (Flash) files
    ///     Written by Michael Swanson (http://blogs.msdn.com/mswanson)
    /// </summary>
    public class SwfReader
    {
        private static readonly uint[] BitValues; // For pre-computed bit values
        private static readonly float[] Powers; // For pre-computed fixed-point powers
        private byte _bitPosition; // Current bit position within byte (only used for reading bit fields)
        private byte _currentByte; // Value of the current byte (only used for reading bit fields)

        #region Properties

        public Stream Stream { get; set; }

        #endregion

        #region Constructors

        static SwfReader()
        {
            // Setup bit values for later lookup
            BitValues = new uint[32];
            for (byte power = 0; power < 32; power++)
            {
                BitValues[power] = (uint) (1 << power);
            }

            // Setup power values for later lookup
            Powers = new float[32];
            for (byte power = 0; power < 32; power++)
            {
                Powers[power] = (float) Math.Pow(2, power - 16);
            }
        }

        public SwfReader(Stream stream)
        {
            Stream = stream;
        }

        #endregion

        #region Stream manipulation

        public byte ReadByte()
        {
            var byteRead = Stream.ReadByte();

            _bitPosition = 8; // So that ReadBit() knows that we've "used" this byte already

            if (byteRead == -1)
            {
                throw new ApplicationException("Attempted to read past end of stream");
            }

            return (byte) byteRead;
        }

        public bool ReadBit()
        {
            // Do we need another byte?
            if (_bitPosition > 7)
            {
                _currentByte = ReadByte();
                _bitPosition = 0; // Reset, since we haven't "used" this byte yet
            }

            // Read the current bit
            var result = ((_currentByte & BitValues[(7 - _bitPosition)]) != 0);

            // Move to the next bit
            _bitPosition++;

            return result;
        }

        #endregion

        #region Byte-aligned types (SI8, SI16, SI32, UI8, UI16, UI32, FIXED, STRING, ARGB[])

        // Read an unsigned 8-bit integer
        public byte ReadUI8()
        {
            return ReadByte();
        }

        // Read an array of unsigned 8-bit integers
        public byte[] ReadUI8(int n)
        {
            var result = new byte[n];

            for (var index = 0; index < n; index++)
            {
                result[index] = ReadUI8();
            }

            return result;
        }

        // Read a signed byte
        public sbyte ReadSI8()
        {
            return (sbyte) ReadByte();
        }

        // Read an unsigned 16-bit integer
        public ushort ReadUI16()
        {
            ushort result = 0;

            result |= ReadByte();
            result |= (ushort) (ReadByte() << 8);

            return result;
        }

        // Read a signed 16-bit integer
        public short ReadSI16() => (short) ReadUI16();

        // Read an unsigned 32-bit integer
        public uint ReadUI32()
        {
            uint result = 0;

            result |= ReadByte();
            result |= (uint) (ReadByte() << 8);
            result |= (uint) (ReadByte() << 16);
            result |= (uint) (ReadByte() << 24);

            return result;
        }

        // Read a signed 32-bit integer
        public int ReadSI32()
        {
            return (int) ReadUI32();
        }

        // Read a 32-bit 16.16 fixed-point number
        public float ReadFIXED()
        {
            float result = 0;

            result += ReadByte()*Powers[0];
            result += ReadByte()*Powers[7];
            result += ReadByte()*Powers[15];
            result += ReadByte()*Powers[31];

            return result;
        }

        // Read a string
        // TODO: Is StringBuilder worth it for these small strings?
        public string ReadSTRING()
        {
            var result = string.Empty;
            byte[] character = {0x00};

            // Grab characters until we hit 0x00
            do
            {
                character[0] = ReadByte();
                if (character[0] != 0x00)
                {
                    result += Encoding.ASCII.GetString(character);
                }
            } while (character[0] != 0x00);

            return result;
        }

        #endregion

        #region Non-byte-aligned bit types (SB[nBits], UB[nBits], FB[nBits])

        // Read an unsigned bit value
        public uint ReadUB(int nBits)
        {
            uint result = 0;

            // Is there anything to read?
            if (nBits > 0)
            {
                // Calculate value
                for (var index = nBits - 1; index > -1; index--)
                {
                    if (ReadBit())
                    {
                        result |= BitValues[index];
                    }
                }
            }

            return result;
        }

        // Read a signed bit value
        public int ReadSB(int nBits)
        {
            var result = 0;

            // Is there anything to read?
            if (nBits > 0)
            {
                // Is this a negative number (MSB will be set)?
                if (ReadBit())
                {
                    result -= (int) BitValues[nBits - 1];
                }

                // Calculate rest of value
                for (var index = nBits - 2; index > -1; index--)
                {
                    if (ReadBit())
                    {
                        result |= (int) BitValues[index];
                    }
                }
            }

            return result;
        }

        // Read a signed fixed-point bit value
        // TODO: Math.Pow probably isn't the fastest method of accomplishing this
        public float ReadFB(int nBits)
        {
            float result = 0;

            // Is there anything to read?
            if (nBits > 0)
            {
                // Is this a negative number (MSB will be set)?
                if (ReadBit())
                {
                    result -= Powers[nBits - 1];
                }

                // Calculate rest of value
                for (var index = nBits - 1; index > 0; index--)
                {
                    if (ReadBit())
                    {
                        result += Powers[index - 1];
                    }
                }
            }

            return result;
        }

        #endregion
    }
}