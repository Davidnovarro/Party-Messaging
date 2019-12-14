using System;
using Party.Messaging.Serialization;

namespace Party.Messaging
{
    public interface IMessageDeserializer
    {
        void Deserialize(NetworkConnection connection, NetworkReader reader);
    }

    public interface IMessagePublisher<T> where T : IMessage
    {
        event Action<NetworkConnection, T> Listeners;
    }
    
    public class MessageHandler<T> : IMessageDeserializer, IMessagePublisher<T> where T : IMessage
    {
        public event Action<NetworkConnection, T> Listeners;

        private readonly MessageSerializer Serializer;

        public MessageHandler(MessageSerializer _serializer, Action<NetworkConnection, T> listener)
        {
            Serializer = _serializer;
            Listeners = listener;
        }

        public void Deserialize(NetworkConnection connection, NetworkReader reader)
        {
            Listeners?.Invoke(connection, Serializer.Deserialize<T>(reader));
        }
    }

}

