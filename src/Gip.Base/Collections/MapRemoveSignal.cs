using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class MapRemoveSignal<TKey, TValue> : MapSignal<TKey, TValue>
        where TKey : notnull
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public MapRemoveSignal(TKey key)
        {
            Key = key;
        }

        /// <summary>
        /// Gets the key to remove.
        /// </summary>
        [ProtoMember(1)]
        public TKey Key { get; }

    }

}
