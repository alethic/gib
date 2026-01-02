using System;

namespace Gib.Base.Collections
{

    public class ListRemoveEvent<T> : ListEvent<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="index"></param>
        public ListRemoveEvent(Index index)
        {
            Index = index;
        }

        /// <summary>
        /// Gets the index at which the item is removed.
        /// </summary>
        public Index Index { get; }

    }

}