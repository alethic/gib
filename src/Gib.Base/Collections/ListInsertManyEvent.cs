using System;
using System.Collections.Immutable;

namespace Gib.Base.Collections
{

    public class ListInsertManyEvent<T> : ListEvent<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="items"></param>
        public ListInsertManyEvent(Index index, ImmutableArray<T> items)
        {
            Index = index;
            Items = items;
        }

        /// <summary>
        /// Gets the index at which the insert happens.
        /// </summary>
        public Index Index { get; }

        /// <summary>
        /// Gets the set of items being inserted.
        /// </summary>
        public ImmutableArray<T> Items { get; }

    }

}