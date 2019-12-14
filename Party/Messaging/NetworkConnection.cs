using Party.Utility;

namespace Party.Messaging
{
    public class NetworkConnection
    {

        public const double RECEIVED_MESSAGES_TRACKING_PERIOUD = 5d;

        public readonly int ConnectionId;

        public readonly ExponentialMovingAverage Ping = new ExponentialMovingAverage(20);

        public int ReceivedMessagesInPeriod { get; private set; }

        public double LastInboundTimestamp { get; private set; }

        internal double currentPingTimestamp;

        internal double currentPingAttempts;

        private int currentReceivedMessagesInPeriod;

        private double lastPeriodResetTime;

        internal void OnMessageReceived()
        {
            LastInboundTimestamp = Time.time;

            if (Time.ElapsedSince(lastPeriodResetTime) > RECEIVED_MESSAGES_TRACKING_PERIOUD)
            {
                ReceivedMessagesInPeriod = currentReceivedMessagesInPeriod;

                lastPeriodResetTime = LastInboundTimestamp;
                currentReceivedMessagesInPeriod = 0;
            }
            
            currentReceivedMessagesInPeriod++;
        }

        public NetworkConnection(int connectionId)
        {
            ConnectionId = connectionId;
            LastInboundTimestamp = Time.time;
        }

        public override string ToString()
        {
            return "Connection : " + ConnectionId;
        }
    }
}
