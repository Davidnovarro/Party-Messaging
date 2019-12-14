using System.IO;
using ProtoBuf;

namespace Party.Messaging.Serialization.Protobuf
{
    public class ProtobufSerializer : MessageSerializer
    {
        public override void Serialize<T>(Stream writer, T item)
        {
            Serializer.SerializeWithLengthPrefix(writer, item, PrefixStyle.Base128);
        }

        public override T Deserialize<T>(Stream reader)
        {
            return Serializer.DeserializeWithLengthPrefix<T>(reader, PrefixStyle.Base128);
        }
    }
}

