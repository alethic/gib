namespace Gib.Base.Collections
{

    public class SetRemoveEvent<T> : SetEvent<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="item"></param>
        public SetRemoveEvent(T item)
        {
            Item = item;
        }

        /// <summary>
        /// Gets the item that was removed.
        /// </summary>
        public T Item { get; }

    }

}