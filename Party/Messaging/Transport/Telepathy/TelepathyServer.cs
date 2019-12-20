using Party.Messaging.Serialization;
using System;
using System.Collections.Generic;
using Telepathy;

namespace Party.Messaging.Transport.Telepathy
{
    public class TelepathyServer : NetworkServer
    {

        internal readonly global::Telepathy.Server Server = new global::Telepathy.Server();

        public override bool Active
        {
            get
            {                
                return Server.Active;
            }
        }

        public TelepathyServer() : base() { }

        public TelepathyServer(MessageSerializer serializer) : base(serializer) { }

        public override void Start(ushort port)
        {
            Server.Start(port);
        }

        public override void Update()
        {
            base.Update();
            // grab all new messages. do this in your Update loop.
            Message msg;
            while (Server.GetNextMessage(out msg))
            {
                switch (msg.eventType)
                {
                    case EventType.Connected:
                        RaiseOnConnected(msg.connectionId);
                        break;
                    case EventType.Data:
                        TransportReceive(msg.connectionId, new ArraySegment<byte>(msg.data));
                        break;
                    case EventType.Disconnected:
                        RaiseOnDisconnected(msg.connectionId);
                        break;
                }
            }
        }

        protected override bool Send(IEnumerable<NetworkConnection> connections, ArraySegment<byte> data)
        {
            var arr = new byte[data.Count];
            Array.Copy(data.Array, data.Offset, arr, 0, data.Count);

            bool r = true;
            foreach (var c in connections)
            {
                r &= Server.Send(c.ConnectionId, arr);
            }
            return r;
        }

        protected override bool TransportSend(int connectionId, ArraySegment<byte> data)
        {
            var arr = new byte[data.Count];
            Array.Copy(data.Array, data.Offset, arr, 0, data.Count);
            return Server.Send(connectionId, arr);
        }

        public override bool TransportDisconnect(int connectionId)
        {
            return Server.Disconnect(connectionId);
        }
    }
}
