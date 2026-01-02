using System;

namespace Gib.Base.Collections
{

    public class ListSetEvent<T> : ListEvent<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public ListSetEvent(Index index, T item)
        {
            Index = index;
            Item = item;
        }

        /// <summary>
        /// Gets the index whose value is set.
        /// </summary>
        public Index Index { get; }

        /// <summary>
        /// Gets the item that is set.
        /// </summary>
        public T Item { get; }

    }

}