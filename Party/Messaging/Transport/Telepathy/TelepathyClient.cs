using Party.Messaging.Serialization;
using System;
using Telepathy;

namespace Party.Messaging.Transport.Telepathy
{
    public class TelepathyClient : NetworkClient
    {

        internal readonly Client Client = new Client();
        
        public override bool Connected
        {
            get
            {
                return base.Connected && Client.Connected;
            }
        }

        public TelepathyClient() : base() { }

        public TelepathyClient(MessageSerializer serializer) : base(serializer) { }

        public override void Connect(string ip, ushort port)
        {
            Client.Connect(ip, port);
        }

        public override void Update()
        {
            base.Update();
            // grab all new messages. do this in your Update loop.
            Message msg;
            while (Client.GetNextMessage(out msg))
            {
                switch (msg.eventType)
                {
                    case EventType.Connected:
                        RaiseOnConnected();
                        break;
                    case EventType.Data:
                        TransportReceive(0, new ArraySegment<byte>(msg.data));
                        break;
                    case EventType.Disconnected:
                        RaiseOnDisconnected();
                        break;
                }
            }
        }

        public override void Disconnect()
        {
            Client.Disconnect();
            RaiseOnDisconnected();
        }

        protected override bool Send(ArraySegment<byte> data)
        {
            var arr = new byte[data.Count];
            Array.Copy(data.Array, data.Offset, arr, 0, data.Count);
            return Client.Send(arr);
        }
    }
}
