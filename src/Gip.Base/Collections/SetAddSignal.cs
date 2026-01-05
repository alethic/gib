using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class SetAddSignal<T> : SetSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="item"></param>
        public SetAddSignal(T item)
        {
            Item = item;
        }

        /// <summary>
        /// Gets the item that was added.
        /// </summary>
        [ProtoMember(1)]
        public T Item { get; }

    }

}
