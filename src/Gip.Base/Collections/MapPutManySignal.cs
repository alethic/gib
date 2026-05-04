using System.Collections.Immutable;

using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class MapPutManySignal<TKey, TValue> : MapSignal<TKey, TValue>
        where TKey : notnull
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="items"></param>
        public MapPutManySignal(ImmutableDictionary<TKey, TValue> items)
        {
            Items = items;
        }

        /// <summary>
        /// Gets the items that were added.
        /// </summary>
        [ProtoMember(1)]
        public ImmutableDictionary<TKey, TValue> Items { get; }

    }

}
