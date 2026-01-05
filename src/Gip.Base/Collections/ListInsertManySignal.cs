using System;
using System.Collections.Immutable;

using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class ListInsertManySignal<T> : ListSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="items"></param>
        public ListInsertManySignal(Index index, ImmutableArray<T> items)
        {
            Index = index;
            Items = items;
        }

        /// <summary>
        /// Gets the index at which the insert happens.
        /// </summary>
        [ProtoMember(1)]
        public Index Index { get; }

        /// <summary>
        /// Gets the set of items being inserted.
        /// </summary>
        [ProtoMember(2)]
        public ImmutableArray<T> Items { get; }

    }

}