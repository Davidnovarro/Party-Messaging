using System.IO;

namespace Party.Messaging.Serialization
{

    public abstract class MessageSerializer
    {
        public virtual void WriteMessage<T>(Stream writer, T item) where T : IMessage
        {
            WriteId(writer, item);
            Serialize(writer, item);
        }

        public virtual void WriteId<T>(Stream writer, T item) where T : IMessage
        {
            writer.WriteUInt16(item.GetId);
        }

        public virtual ushort ReadId(Stream reader)
        {
            try
            {
                return reader.ReadUInt16();
            }
            catch (EndOfStreamException)
            {
                return 0;
            }
        }

        public abstract void Serialize<T>(Stream writer, T item) where T : IMessage;

        public abstract T Deserialize<T>(Stream reader) where T : IMessage;
    }
    
}

