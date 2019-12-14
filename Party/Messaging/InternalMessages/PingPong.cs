using ProtoBuf;

namespace Party.Messaging.InternalMessages
{
    [ProtoContract]
    public struct PingMessage : IMessage
    {
        public ushort GetId { get { return _mid; } }

        static readonly ushort _mid = MessageIdProvider.GetId<PingMessage>();

        [ProtoMember(1)]
        public double pingTimestamp;
        
        public PingMessage(double ping)
        {
            pingTimestamp = ping;
        }

    }

    [ProtoContract]
    public struct PongMessage : IMessage
    {
        public ushort GetId { get { return _mid; } }

        static readonly ushort _mid = MessageIdProvider.GetId<PongMessage>();

        [ProtoMember(1)]
        public double pingTimestamp;

        public PongMessage(double ping)
        {
            pingTimestamp = ping;
        }
    }
}
