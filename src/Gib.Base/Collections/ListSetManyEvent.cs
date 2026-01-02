using System;
using System.Collections.Immutable;

namespace Gib.Base.Collections
{

    public class ListSetManyEvent<T> : ListEvent<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="items"></param>
        public ListSetManyEvent(Index index, ImmutableArray<T> items)
        {
            Index = index;
            Items = items;
        }

        /// <summary>
        /// Gets the index at which the set begins.
        /// </summary>
        public Index Index { get; }

        /// <summary>
        /// Gets the items to set at the index.
        /// </summary>
        public ImmutableArray<T> Items { get; }

    }

}