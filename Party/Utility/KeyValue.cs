namespace Party.Utility
{
    [System.Serializable]
    public struct KeyValue<TKey, TValue>
    {
        public TKey key;
        public TValue value;

        public KeyValue(TKey _key, TValue _value)
        {
            key = _key;
            value = _value;
        }
    }
}


