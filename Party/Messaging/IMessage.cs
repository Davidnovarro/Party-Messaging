namespace Party.Messaging
{
    public interface IMessage
    {
        ushort GetId { get; }
    }

    public interface IRequestMessage : IMessage
    {
        ushort QueryId { get; set; }
    }

    public interface IResponseMessage : IMessage
    {
        ushort QueryId { get; set; }
    }
}
