using System;
using Party.Utility;
using System.Threading;
using System.Collections.Concurrent;
using Party.Messaging.Serialization;
using Party.Messaging.InternalMessages;
using Party.Messaging.Serialization.Protobuf;

namespace Party.Messaging
{
    public abstract class NetworkBase
    {
        private readonly object _lock = new object();

        protected readonly ConcurrentDictionary<ushort, IMessageDeserializer> Handlers = new ConcurrentDictionary<ushort, IMessageDeserializer>();
        
        protected readonly ThreadLocal<NetworkReader> Reader = new ThreadLocal<NetworkReader>(() => new NetworkReader(new byte[0]));

        protected readonly ThreadLocal<NetworkWriter> Writer = new ThreadLocal<NetworkWriter>(() => new NetworkWriter());

        protected readonly MessageSerializer Serializer;

        public readonly NetworkKeepAlive KeepAlive;

        public readonly NetworkQueries Queries;

        public NetworkBase() : this(new ProtobufSerializer()) { }

        public NetworkBase(MessageSerializer serializer)
        {
            Serializer = serializer;
            KeepAlive = new NetworkKeepAlive(this);
            Queries = new NetworkQueries(this);
            AddListener<ErrorMessage>(OnErrorMessage);
        }

        public virtual void Update()
        {
            Queries.TimeoutCheck();
        }

        protected abstract bool TransportSend(int connectionId, ArraySegment<byte> data);

        public abstract void TransportDisconnect(int connectionId);

        public virtual bool Send<T>(NetworkConnection connection, T msg) where T : IMessage
        {
            var w = Writer.Value;
            Serializer.WriteMessage(w, msg);
            if (TransportSend(connection.ConnectionId, w.ToArraySegment()))
            {
                w.Clear();
                return true;
            }
            else
            {
                w.Clear();
                return false;
            }
        }

        public void AddListener<T>(Action<NetworkConnection, T> listener) where T : IMessage
        {
            ushort msgId = MessageIdProvider.GetId<T>();

            //We need to lock because we will do multiple actions with the dict
            lock (_lock)
            {
                IMessageDeserializer handler;
                if (!Handlers.TryGetValue(msgId, out handler))
                {
                    //No message message registered yet, adding a new invoker
                    handler = new MessageHandler<T>(Serializer, listener);
                    Handlers[msgId] = handler;
                }
                else
                {
                    (handler as IMessagePublisher<T>).Listeners += listener;
                }
            }
        }

        public void RemoveListener<T>(Action<NetworkConnection, T> listener) where T : IMessage
        {
            ushort msgId = MessageIdProvider.GetId<T>();

            //We need to lock because we will do multiple actions with the dict
            lock (_lock)
            {
                IMessageDeserializer handler;
                if (Handlers.TryGetValue(msgId, out handler))
                {
                    (handler as IMessagePublisher<T>).Listeners -= listener;
                }
            }
        }

        protected void TransportReceive(int connectionId, ArraySegment<byte> data)
        {
            var r = Reader.Value;
            r.Reload(data);

            while (r.Position < r.Length)
            {
                ushort msgId = Serializer.ReadId(r);

                if (msgId == 0)
                {
                    Logger.Warning("Message ID, no message is received");
                    TransportDisconnect(connectionId);
                    return;
                }

                InvokeHandler(connectionId, msgId, r);
            }
        }

        protected abstract void InvokeHandler(int connectionId, ushort msgId, NetworkReader reader);
     
        protected virtual void OnErrorMessage(NetworkConnection conn, ErrorMessage message)
        {
            Logger.Error("Error message received " + conn.ToString() + " " + message.ToString());
        }

    }
}