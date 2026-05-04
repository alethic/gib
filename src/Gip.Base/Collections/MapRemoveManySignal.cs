using System.Collections.Immutable;

using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class MapRemoveManySignal<TKey, TValue> : MapSignal<TKey, TValue>
        where TKey : notnull
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="keys"></param>
        public MapRemoveManySignal(ImmutableArray<TKey> keys)
        {
            Keys = keys;
        }

        /// <summary>
        /// Gets the keys to remove.
        /// </summary>
        [ProtoMember(1)]
        public ImmutableArray<TKey> Keys { get; }

    }

}
