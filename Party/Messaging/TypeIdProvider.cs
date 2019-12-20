using System;
using System.Collections.Concurrent;

namespace Party.Messaging
{
    public static class TypeIdProvider
    {

        private static readonly ConcurrentDictionary<Type, ushort> IdDict = new ConcurrentDictionary<Type, ushort>();

        private static readonly object _lock = new object();

        public static ushort lastQueryId;

        private static int GetStableHashCode(this string text)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in text)
                    hash = hash * 31 + c;
                return hash;
            }
        }

        public static ushort GetId<T>() where T : IMessage
        {
            return GetId(typeof(T));
        }

        public static ushort GetId(Type type)
        {
            ushort id;
            if (!IdDict.TryGetValue(type, out id))
            {

                // paul: 16 bits is enough to avoid collisions
                //  - keeps the message size small because it gets varinted
                //  - in case of collisions,  Mirror will display an error
                id = (ushort)(type.FullName.GetStableHashCode() & 0xFFFF);

                foreach (var item in IdDict)
                {
                    if (item.Value == id && item.Key != type)
                    {
                        throw new Exception(string.Format("IDs are same for types : {0} and {1}", item.Key, type));
                    }
                }

                IdDict[type] = id;
            }

            return id;
        }

        public static ushort GetNextQueryId()
        {
            ushort r;
            lock (_lock)
            {
                lastQueryId++;
                if (lastQueryId == 0)
                    lastQueryId++;

                r = lastQueryId;
            }

            return r;
        }

        public static bool HasCallback(this IRequestMessage request)
        {
            return request.QueryId != 0;
        }

    }
}
