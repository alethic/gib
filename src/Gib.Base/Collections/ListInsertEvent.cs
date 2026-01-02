using System;

namespace Gib.Base.Collections
{

    public class ListInsertEvent<T> : ListEvent<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public ListInsertEvent(Index index, T item)
        {
            Index = index;
            Item = item;
        }

        /// <summary>
        /// Gets the index at which the item is inserted.
        /// </summary>
        public Index Index { get; }

        /// <summary>
        /// Gets the item that is inserted.
        /// </summary>
        public T Item { get; }

    }

}