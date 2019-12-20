using System;
using Party.Utility;
using System.Collections.Concurrent;
using Party.Messaging.InternalMessages;

namespace Party.Messaging
{
    public class NetworkQueries : Module<NetworkBase>
    {

        private ConcurrentDictionary<ushort, QueryResponsePublisher> queryHandlers = new ConcurrentDictionary<ushort, QueryResponsePublisher>();

        private double lastTimeoutCheck;

        public NetworkQueries(NetworkBase manager) : base(manager) { }
        
        public void AddListener<T>(Action<NetworkConnection, T> listener) where T : IRequestMessage
        {
            Manager.AddListener<T>(listener);
        }

        public void RemoveListener<T>(Action<NetworkConnection, T> listener) where T : IRequestMessage
        {
            Manager.RemoveListener<T>(listener);
        }

        public bool Request<TRequest, TResponse>(NetworkConnection conn, TRequest request, Action<NetworkConnection, TResponse> callback) where TRequest : IRequestMessage where TResponse : IResponseMessage
        {
            return Request(conn, request, callback, LogError, 5d);
        }

        public bool Request<TRequest, TResponse>(NetworkConnection conn, TRequest request, Action<NetworkConnection, TResponse> callback, Action<NetworkConnection, ErrorMessage> errorCallback, double timeOut) where TRequest : IRequestMessage where TResponse : IResponseMessage
        {
            if (callback != null || errorCallback != null)
            {
                QueryResponsePublisher pub;

                if (!queryHandlers.TryGetValue(request.GetId, out pub))
                {
                    pub = new QueryResponsePublisher();

                    Manager.AddListener<TResponse>((c, r) =>
                    {
                        pub.PublishQueryResponse(c, r.QueryId, r);
                    });

                    if (!queryHandlers.TryAdd(request.GetId, pub))
                    {
                        Logger.Error("Can't add response publisher to queryHandlers : " + request.GetId);
                    }
                }

                request.QueryId = TypeIdProvider.GetNextQueryId();

                var queryCallback = new QueryCallback<IResponseMessage>((c, m) => callback.Invoke(c, (TResponse)m), errorCallback, Time.time + timeOut);

                pub.AddCallback(request.QueryId, queryCallback);
            }
            else
            {
                request.QueryId = 0;
            }

            return Manager.Send(conn, request);
        }

        public bool Respond<TRequest, TResponse>(NetworkConnection conn, TResponse respose, TRequest request) where TRequest : IRequestMessage where TResponse : IResponseMessage
        {
            if(!request.HasCallback())
            {
                Logger.Warning("Response has no listener callback");
                return false;
            }
            respose.QueryId = request.QueryId;
            return Manager.Send(conn, respose);
        }

        internal static void LogError(NetworkConnection conn, ErrorMessage message)
        {
            Logger.Error("QueryError for connection " + conn?.ConnectionId + " " + message.ToString());
        }
        
        internal void TimeoutCheck()
        {
            if(Time.ElapsedSince(lastTimeoutCheck) > 0.1d)
            {
                lastTimeoutCheck = Time.time;
                foreach (var qh in queryHandlers.Values)
                {
                    qh.TimeoutCheck(lastTimeoutCheck);
                }
            }
        }

    }
    
    internal class QueryResponsePublisher
    {
        private readonly ConcurrentDictionary<ushort, QueryCallback<IResponseMessage>> queries = new ConcurrentDictionary<ushort, QueryCallback<IResponseMessage>>();

        internal void AddCallback(ushort queryId, QueryCallback<IResponseMessage> callback)
        {
            if (!queries.TryAdd(queryId, callback))
            {
                Logger.Error("Can't add query, QueryId already exists");
            }
        }

        /// <summary>
        /// Use this to remove old listeners that did not get any responses. (Timed out etc...)
        /// </summary>
        internal void TimeoutCheck(double time)
        {
            foreach (var q in queries)
            {
                QueryCallback<IResponseMessage> qa;
                if (q.Value.timeoutTimestamp < time)
                {
                    if (!queries.TryRemove(q.Key, out qa) && queries.Keys.Contains(q.Key))
                        Logger.Error("Can't remove query");
                    else
                    {
                        q.Value.ErrorCallback?.Invoke(null, new ErrorMessage() { code = ErrorCode.RequestTimeout });
                    }
                }
            }
        }

        internal void PublishQueryResponse(NetworkConnection connection, ushort queryId, IResponseMessage response)
        {
            QueryCallback<IResponseMessage> q;
            if (queries.TryRemove(queryId, out q))
            {
                q.ResponseCallback?.Invoke(connection, response);
                return;
            }
            else
            {
                Logger.Error("Query response has no listener : messageId : " + response.GetId + " queryId : " + queryId);
            }
        }
    }

    internal struct QueryCallback<TResponse> where TResponse : IResponseMessage
    {
        public readonly Action<NetworkConnection, TResponse> ResponseCallback;

        public readonly Action<NetworkConnection, ErrorMessage> ErrorCallback;

        public double timeoutTimestamp;

        public QueryCallback(Action<NetworkConnection, TResponse> response, Action<NetworkConnection, ErrorMessage> errorCallback, double timeoutTime)
        {
            timeoutTimestamp = timeoutTime;
            ResponseCallback = response;
            ErrorCallback = errorCallback;
        }
    }

}
