using System.Collections.Immutable;

namespace Gib.Base.Collections
{

    public class SetAddManyEvent<T> : SetEvent<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="items"></param>
        public SetAddManyEvent(ImmutableHashSet<T> items)
        {
            Items = items;
        }

        /// <summary>
        /// Gets the items that were added.
        /// </summary>
        public ImmutableHashSet<T> Items { get; }

    }

}
