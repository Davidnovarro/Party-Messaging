using ProtoBuf;

namespace Party.Messaging.Examples.Queries
{
    [ProtoContract]
    public struct ExampleRequestMessage : IRequestMessage
    {
        public ushort GetId { get { return _mid; } }
        
        static readonly ushort _mid = TypeIdProvider.GetId<ExampleRequestMessage>();

        [ProtoMember(1)]
        public ushort QueryId { get; set; }

        [ProtoMember(2)]
        public int exampleId;
    }

    [ProtoContract]
    public struct ExampleResponseMessage : IResponseMessage
    {
        public ushort GetId { get { return _mid; } }
        
        static readonly ushort _mid = TypeIdProvider.GetId<ExampleResponseMessage>();

        [ProtoMember(1)]
        public ushort QueryId { get; set; }

        [ProtoMember(2)]
        public int exampleId;

        [ProtoMember(3)]
        public string result;
    }
}
