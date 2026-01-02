using System;

namespace Gib.Base.Collections
{

    public class ListRemoveManyEvent<T> : ListEvent<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="range"></param>
        public ListRemoveManyEvent(Range range)
        {
            Range = range;
        }

        /// <summary>
        /// Gets the range of items to be removed.
        /// </summary>
        public Range Range { get; }

    }

}