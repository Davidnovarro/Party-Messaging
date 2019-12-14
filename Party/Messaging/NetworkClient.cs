using System;
using Party.Utility;
using Party.Messaging.Serialization;

namespace Party.Messaging
{
    public abstract class NetworkClient : NetworkBase
    {
        public NetworkConnection Connection;
               
        public virtual bool Connected { get { return Connection != null; } }

        public event Action OnConnected;

        public event Action OnDisconnected;
        

        public NetworkClient() : base()
        {
            KeepAlive.MaxMessageCountPerPeriod = 250;
        }

        public NetworkClient(MessageSerializer serializer) : base(serializer)
        {
            KeepAlive.MaxMessageCountPerPeriod = 250;
        }

        public abstract void Connect(string ip, ushort port);

        public override void Update()
        {
            base.Update();

            if (Connected)
            {
                KeepAlive.KeepAlive(Connection);
            }                
        }

        public abstract void Disconnect();
        
        protected virtual void RaiseOnConnected()
        {
            Connection = new NetworkConnection(0);
            OnConnected?.Invoke();
        }

        protected virtual void RaiseOnDisconnected()
        {
            OnDisconnected?.Invoke();
        }
               
        public NetworkClient PipeMessage<T>(T msg) where T : IMessage
        {
            Serializer.WriteMessage(Writer.Value, msg);
            return this;
        }

        public bool Send()
        {
            var w = Writer.Value;

            if (w.Position == 0)
            {
                Logger.Error("No pipelined message to send");
                return false;
            }

            if (Send(w.ToArraySegment()))
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

        public bool Send<T>(T msg) where T : IMessage
        {
            var w = Writer.Value;
            Serializer.WriteMessage(w, msg);
            if (Send(w.ToArraySegment()))
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
            
        protected abstract bool Send(ArraySegment<byte> data);

        protected override bool TransportSend(int conn, ArraySegment<byte> data)
        {
            return Send(data);
        }

        public override void TransportDisconnect(int connectionId)
        {
            Disconnect();
        }

        protected override void InvokeHandler(int connectionId, ushort msgId, NetworkReader reader)
        {
            Connection.OnMessageReceived();

            IMessageDeserializer h;

            if(!Handlers.TryGetValue(msgId, out h))
            {
                Logger.Error("Message has no registered listener : " + msgId);
                return;
            }

            h.Deserialize(Connection, reader);
        }

    }
}
