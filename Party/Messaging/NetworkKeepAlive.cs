using Party.Utility;
using System.Collections.Concurrent;
using Party.Messaging.InternalMessages;

namespace Party.Messaging
{
    public class NetworkKeepAlive : Module<NetworkBase>
    {

        public int MaxMessageCountPerPeriod = 300;
   
        public double KeepAliveInterval = 10d;

        public double RetryInterval = 2d;

        public int MaxRetry = 3;

        private double lastBulkKeepAlive;

        public NetworkKeepAlive(NetworkBase manager) : base (manager)
        {
            Manager.AddListener<PingMessage>(OnPingMessage);
            Manager.AddListener<PongMessage>(OnPongMessage);
        }


        public void Destroy()
        {
            Manager.RemoveListener<PingMessage>(OnPingMessage);
            Manager.RemoveListener<PongMessage>(OnPongMessage);
        }
        

        public void KeepAlive(ConcurrentDictionary<int, NetworkConnection> Connections)
        {
            if (Time.ElapsedSince(lastBulkKeepAlive) < 0.2f)
                return;

            lastBulkKeepAlive = Time.time;

            foreach (var conn in Connections.Values)
            {
                KeepAlive(conn);
            }
        }

        public void KeepAlive(NetworkConnection conn)
        {
            if (conn.ReceivedMessagesInPeriod > MaxMessageCountPerPeriod)
            {
                var msg = new ErrorMessage() { code = ErrorCode.MessageSpamming };
                Manager.Send(conn, msg);
                Logger.Error(conn + " is spamming with messages, disconnecting.");
                Manager.TransportDisconnect(conn.ConnectionId);
            }

            if (Time.ElapsedSince(conn.LastInboundTimestamp) < KeepAliveInterval)
                return;

            if (conn.currentPingAttempts <= 0)
            {
                Logger.Debug(string.Format("No inbound data for {0} seconds, Pinging to keep alive", Time.ElapsedSince(conn.LastInboundTimestamp)));
                conn.currentPingAttempts = 0;
                Ping(conn);
                return;
            }
            else if (Time.ElapsedSince(conn.currentPingTimestamp) > RetryInterval)
            {
                if (conn.currentPingAttempts < MaxRetry)
                {
                    Logger.Debug(string.Format("No ping response for {0} seconds, retrying", RetryInterval));
                    Ping(conn);
                }
                else
                {
                    Manager.TransportDisconnect(conn.ConnectionId);
                }
            }
        }
        
        public void Ping(NetworkConnection conn)
        {
            var ping = new PingMessage(Time.time);
            Manager.Send(conn, ping);
            conn.currentPingAttempts++;
            conn.currentPingTimestamp = ping.pingTimestamp;
        }

        protected void OnPingMessage(NetworkConnection conn, PingMessage msg)
        {
            Manager.Send(conn, new PongMessage(msg.pingTimestamp));
        }

        protected void OnPongMessage(NetworkConnection conn, PongMessage msg)
        {
            conn.currentPingTimestamp = 0d;
            conn.currentPingAttempts = 0;
            conn.Ping.Add(Time.time - msg.pingTimestamp);
        }

    }
}
