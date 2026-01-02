namespace Gib.Base.Collections
{

    public class SetAddEvent<T> : SetEvent<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="item"></param>
        public SetAddEvent(T item)
        {
            Item = item;
        }

        /// <summary>
        /// Gets the item that was added.
        /// </summary>
        public T Item { get; }

    }

}