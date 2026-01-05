using System;

using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class ListInsertSignal<T> : ListSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public ListInsertSignal(Index index, T item)
        {
            Index = index;
            Item = item;
        }

        /// <summary>
        /// Gets the index at which the item is inserted.
        /// </summary>
        [ProtoMember(1)]
        public Index Index { get; }

        /// <summary>
        /// Gets the item that is inserted.
        /// </summary>
        [ProtoMember(2)]
        public T Item { get; }

    }

}
