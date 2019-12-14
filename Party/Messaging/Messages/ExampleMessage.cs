using ProtoBuf;

namespace Party.Messaging.Messages
{
    [ProtoContract]
    public struct ExampleMessage : IMessage
    {
        static readonly ushort id = MessageIdProvider.GetId<ExampleMessage>();

        public ushort GetId { get { return id; } }

        [ProtoMember(1)]
        public int senderId;

        [ProtoMember(2)]
        public int receiverId;

        [ProtoMember(3)]
        public string message;
    }
}
