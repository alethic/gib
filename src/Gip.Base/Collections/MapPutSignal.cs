using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class MapPutSignal<TKey, TValue> : MapSignal<TKey, TValue>
        where TKey : notnull
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public MapPutSignal(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Gets the key to add.
        /// </summary>
        [ProtoMember(1)]
        public TKey Key { get; }

        /// <summary>
        /// Gets the value to add.
        /// </summary>
        [ProtoMember(2)]
        public TValue Value { get; }

    }

}
