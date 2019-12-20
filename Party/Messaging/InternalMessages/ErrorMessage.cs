using ProtoBuf;
using System;
using System.Text;

namespace Party.Messaging.InternalMessages
{

    public enum ErrorCode : byte
    {
        Unknown = 0,
        RequestTimeout = 4,
        MessageSpamming = 14
    }

    [ProtoContract]
    public struct ErrorMessage : IMessage
    {
        public ushort GetId { get { return _mid; } }

        static readonly ushort _mid = TypeIdProvider.GetId<ErrorMessage>();

        [ProtoMember(1)]
        public ErrorCode code;

        [ProtoMember(2)]
        public string message;

        [ThreadStatic]
        private static StringBuilder _sb;

        public override string ToString()
        {
            if (_sb == null)
                _sb = new StringBuilder();
                        
            _sb.Append("ErrorCode : " + code);

            if (!string.IsNullOrEmpty(message))
                _sb.Append("\nMessage : " + message);

            var r = _sb.ToString();
            _sb.Clear();
            return r;
        }

    }
}
