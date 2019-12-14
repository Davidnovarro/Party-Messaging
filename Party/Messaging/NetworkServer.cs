using System;
using Party.Utility;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Party.Messaging.Serialization;

namespace Party.Messaging
{
    public abstract class NetworkServer : NetworkBase
    {
        public readonly ConcurrentDictionary<int, NetworkConnection> Connections = new ConcurrentDictionary<int, NetworkConnection>();
        
        public abstract bool Active { get; }
        
        public event Action<NetworkConnection> OnConnected;

        public event Action<NetworkConnection> OnDisconnected;
        
        public NetworkServer() : base()
        {
            KeepAlive.MaxMessageCountPerPeriod = 150;
        }

        public NetworkServer(MessageSerializer serializer) : base(serializer)
        {
            KeepAlive.MaxMessageCountPerPeriod = 150;
        }

        public abstract void Start(ushort port);
        
        public override void Update()
        {
            base.Update();
            KeepAlive.KeepAlive(Connections);
        }

        protected virtual void RaiseOnConnected(int connectionId)
        {
            var conn = new NetworkConnection(connectionId);
            if (!Connections.TryAdd(connectionId, conn))
            {
                Logger.Error("Connection is already exist in dictionary");
            }

            OnConnected?.Invoke(conn);
        }

        protected virtual void RaiseOnDisconnected(int connectionId)
        {
            NetworkConnection conn;
            if (!Connections.TryRemove(connectionId, out conn))
            {
                Logger.Warning("Connection is already removed");
            }

            OnDisconnected?.Invoke(conn);
        }

        public NetworkServer PipeMessage<T>(T msg) where T : IMessage
        {
            Serializer.WriteMessage(Writer.Value, msg);
            return this;
        }

        public bool Send(NetworkConnection connection)
        {
            var w = Writer.Value;

            if(w.Position == 0)
            {
                Logger.Error("No pipelined message to send");
                return false;
            }

            if(TransportSend(connection.ConnectionId, w.ToArraySegment()))
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

        public bool Send(IEnumerable<NetworkConnection> connections)
        {
            var w = Writer.Value;

            if (w.Position == 0)
            {
                Logger.Error("No pipelined message to send");
                return false;
            }

            if(Send(connections, w.ToArraySegment()))
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
        
        public bool Send<T>(IEnumerable<NetworkConnection> connections, T msg) where T : IMessage
        {
            var w = Writer.Value;
            Serializer.WriteMessage(w, msg);
            if(Send(connections, w.ToArraySegment()))
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
        
        protected abstract bool Send(IEnumerable<NetworkConnection> connections, ArraySegment<byte> data);

        protected override void InvokeHandler(int connectionId, ushort msgId, NetworkReader reader)
        {
            var conn = Connections[connectionId];
            conn.OnMessageReceived();

            IMessageDeserializer h;

            if (!Handlers.TryGetValue(msgId, out h))
            {
                Logger.Error("Message has no registered listener : " + msgId);
                return;
            }

            h.Deserialize(conn, reader);
        }
        
    }
}
