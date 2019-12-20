using System;
using System.IO;
using System.Text;

namespace Party.Messaging
{
    public class NetworkReader : Stream
    {

        static readonly UTF8Encoding encoding = new UTF8Encoding(false, true);

        protected ArraySegment<byte> buffer;


        public override long Position { get; set; }

        public override long Length => buffer.Count;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public NetworkReader(byte[] bytes)
        {
            buffer = new ArraySegment<byte>(bytes);
        }

        public NetworkReader(ArraySegment<byte> segment)
        {
            buffer = segment;
        }


        #region Methods

        public override int ReadByte()
        {
            if (Position + 1 > buffer.Count)
            {
                throw new EndOfStreamException("ReadByte out of range:" + ToString());
            }
            return buffer.Array[buffer.Offset + Position++];
        }
        public int ReadInt32() => (int)ReadUInt32();
        public uint ReadUInt32()
        {
            uint value = 0;
            value |= (uint)ReadByte();
            value |= (uint)(ReadByte() << 8);
            value |= (uint)(ReadByte() << 16);
            value |= (uint)(ReadByte() << 24);
            return value;
        }
        public long ReadInt64() => (long)ReadUInt64();
        public ulong ReadUInt64()
        {
            ulong value = 0;
            value |= (uint)ReadByte();
            value |= ((ulong)ReadByte()) << 8;
            value |= ((ulong)ReadByte()) << 16;
            value |= ((ulong)ReadByte()) << 24;
            value |= ((ulong)ReadByte()) << 32;
            value |= ((ulong)ReadByte()) << 40;
            value |= ((ulong)ReadByte()) << 48;
            value |= ((ulong)ReadByte()) << 56;
            return value;
        }


        public sbyte ReadSByte() => (sbyte)ReadByte();
        public char ReadChar() => (char)ReadUInt16();
        public bool ReadBoolean() => ReadByte() != 0;
        public short ReadInt16() => (short)ReadUInt16();
        public ushort ReadUInt16()
        {
            ushort value = 0;
            value |= (ushort)ReadByte();
            value |= (ushort)(ReadByte() << 8);
            return value;
        }
        public float ReadSingle()
        {
            return new UIntFloat { intValue = ReadUInt32() }.floatValue;
        }
        public double ReadDouble()
        {
            return new UIntDouble { longValue = ReadUInt64() }.doubleValue;
        }
        public decimal ReadDecimal()
        {
            return new UIntDecimal { longValue1 = ReadUInt64(), longValue2 = ReadUInt64() }.decimalValue;
        }

        public string ReadString()
        {
            // read number of bytes
            ushort size = ReadUInt16();

            if (size == 0)
                return null;

            int realSize = size - 1;

            // make sure it's within limits to avoid allocation attacks etc.
            if (realSize >= NetworkWriter.MaxStringLength)
            {
                throw new EndOfStreamException("ReadString too long: " + realSize + ". Limit is: " + NetworkWriter.MaxStringLength);
            }

            ArraySegment<byte> data = ReadBytesSegment(realSize);

            // convert directly from buffer to string via encoding
            return encoding.GetString(data.Array, data.Offset, data.Count);
        }

        public byte[] ReadBytesAndSize()
        {
            // count = 0 means the array was null
            // otherwise count -1 is the length of the array 
            uint count = ReadPackedUInt32();
            return count == 0 ? null : ReadBytes(checked((int)(count - 1u)));
        }

        public ArraySegment<byte> ReadBytesAndSizeSegment()
        {
            // count = 0 means the array was null
            // otherwise count - 1 is the length of the array
            uint count = ReadPackedUInt32();
            return count == 0 ? default(ArraySegment<byte>) : ReadBytesSegment(checked((int)(count - 1u)));
        }

        // zigzag decoding https://gist.github.com/mfuerstenau/ba870a29e16536fdbaba
        public int ReadPackedInt32()
        {
            uint data = ReadPackedUInt32();
            return (int)((data >> 1) ^ -(data & 1));
        }

        public uint ReadPackedUInt32() => checked((uint)ReadPackedUInt64());

        // zigzag decoding https://gist.github.com/mfuerstenau/ba870a29e16536fdbaba
        public long ReadPackedInt64()
        {
            ulong data = ReadPackedUInt64();
            return ((long)(data >> 1)) ^ -((long)data & 1);
        }

        public ulong ReadPackedUInt64()
        {
            var a0 = (ulong)ReadByte();
            if (a0 < 241)
            {
                return a0;
            }

            var a1 = (ulong)ReadByte();
            if (a0 >= 241 && a0 <= 248)
            {
                return 240 + ((a0 - (ulong)241) << 8) + a1;
            }

            var a2 = (ulong)ReadByte();
            if (a0 == 249)
            {
                return 2288 + ((ulong)a1 << 8) + a2;
            }

            var a3 = (ulong)ReadByte();
            if (a0 == 250)
            {
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16);
            }

            var a4 = (ulong)ReadByte();
            if (a0 == 251)
            {
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24);
            }

            var a5 = (ulong)ReadByte();
            if (a0 == 252)
            {
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32);
            }

            var a6 = (ulong)ReadByte();
            if (a0 == 253)
            {
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32) + (((ulong)a6) << 40);
            }

            var a7 = (ulong)ReadByte();
            if (a0 == 254)
            {
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32) + (((ulong)a6) << 40) + (((ulong)a7) << 48);
            }

            var a8 = (ulong)ReadByte();
            if (a0 == 255)
            {
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32) + (((ulong)a6) << 40) + (((ulong)a7) << 48) + (((ulong)a8) << 56);
            }

            throw new IndexOutOfRangeException("ReadPackedUInt64() failure: " + a0);
        }


        public byte[] ReadBytes(int count)
        {
            byte[] bytes = new byte[count];
            ReadBytes(bytes, count);
            return bytes;
        }

        public Guid ReadGuid() => new Guid(ReadBytes(16));

        public byte[] ReadBytes(byte[] bytes, int count)
        {
            // check if passed byte array is big enough
            if (count > bytes.Length)
            {
                throw new EndOfStreamException("ReadBytes can't read " + count + " + bytes because the passed byte[] only has length " + bytes.Length);
            }

            ArraySegment<byte> data = ReadBytesSegment(count);
            Array.Copy(data.Array, data.Offset, bytes, 0, count);
            return bytes;
        }


        public ArraySegment<byte> ReadBytesSegment(int count)
        {
            // check if within buffer limits
            if (Position + count > buffer.Count)
            {
                throw new EndOfStreamException("ReadBytesSegment can't read " + count + " bytes because it would read past the end of the stream. " + ToString());
            }

            // return the segment
            ArraySegment<byte> result = new ArraySegment<byte>(buffer.Array, (int)(buffer.Offset + Position), count);
            Position += count;
            return result;
        }

        public override string ToString()
        {
            return "NetworkReader pos=" + Position + " len=" + Length + " buffer=" + BitConverter.ToString(buffer.Array, buffer.Offset, buffer.Count);
        }

        #endregion



        public override void Flush() { }

        public override int Read(byte[] buff, int offset, int count)
        {
            if (buff == null)
                throw new ArgumentNullException("buffer", "ArgumentNull_Buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "ArgumentOutOfRange_NeedNonNegNum");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
            if (buff.Length - offset < count)
                throw new ArgumentException("Argument_InvalidOffLen");


            count = Math.Min(count, buffer.Count - (int)Position);

            Array.Copy(buffer.Array, Position, buff, offset, count);

            Position += count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (offset > Length)
                throw new ArgumentOutOfRangeException("offset", "ArgumentOutOfRange_StreamLength");
            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        int tempPosition = (int)offset;
                        if (offset < 0 || tempPosition < 0)
                            throw new IOException("IO.IO_SeekBeforeBegin");
                        Position = tempPosition;
                        break;
                    }
                case SeekOrigin.Current:
                    {
                        int tempPosition = unchecked((int)Position + (int)offset);
                        if (unchecked(Position + offset) < 0 || tempPosition < 0)
                            throw new IOException("IO.IO_SeekBeforeBegin");
                        Position = tempPosition;
                        break;
                    }
                case SeekOrigin.End:
                    {
                        int tempPosition = unchecked((int)Length + (int)offset);
                        if (unchecked(Length + offset) < 0 || tempPosition < 0)
                            throw new IOException("IO.IO_SeekBeforeBegin");
                        Position = tempPosition;
                        break;
                    }
                default:
                    throw new ArgumentException("Argument_InvalidSeekOrigin");
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException("Can't set the length of read only stream");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException("Can't write into read only stream");
        }

        public void Reload(byte[] array)
        {
            buffer = new ArraySegment<byte>(array);
            Position = 0;
        }

        public void Reload(ArraySegment<byte> segment)
        {
            buffer = segment;
            Position = 0;
        }
    }
}
