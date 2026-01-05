using System;
using System.Collections.Immutable;

using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class ListSetManySignal<T> : ListSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="items"></param>
        public ListSetManySignal(Index index, ImmutableArray<T> items)
        {
            Index = index;
            Items = items;
        }

        /// <summary>
        /// Gets the index at which the set begins.
        /// </summary>
        [ProtoMember(1)]
        public Index Index { get; }

        /// <summary>
        /// Gets the items to set at the index.
        /// </summary>
        [ProtoMember(2)]
        public ImmutableArray<T> Items { get; }

    }

}