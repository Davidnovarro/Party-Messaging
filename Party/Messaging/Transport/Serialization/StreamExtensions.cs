using System.IO;

namespace Party.Messaging.Serialization
{
    public static class StreamExtensions
    {

        public static void WriteUInt16(this Stream stream, ushort value)
        {
            stream.WriteByte((byte)(value & 0xFF));
            stream.WriteByte((byte)(value >> 8));
        }


        public static ushort ReadUInt16(this Stream stream)
        {
            ushort value = 0;
            value |= (ushort)stream.ReadByte();
            value |= (ushort)(stream.ReadByte() << 8);
            return value;
        }

    }
}
