using System.Collections.Immutable;

namespace Gib.Base.Collections
{

    public class SetRemoveManyEvent<T> : SetEvent<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="items"></param>
        public SetRemoveManyEvent(ImmutableHashSet<T> items)
        {
            Items = items;
        }

        /// <summary>
        /// Gets the items that were removed.
        /// </summary>
        public ImmutableHashSet<T> Items { get; }

    }

}