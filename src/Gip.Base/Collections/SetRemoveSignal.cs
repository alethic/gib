using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class SetRemoveSignal<T> : SetSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="item"></param>
        public SetRemoveSignal(T item)
        {
            Item = item;
        }

        /// <summary>
        /// Gets the item that was removed.
        /// </summary>
        [ProtoMember(1)]
        public T Item { get; }

    }

}
