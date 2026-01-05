using System;

using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class ListSetSignal<T> : ListSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public ListSetSignal(Index index, T item)
        {
            Index = index;
            Item = item;
        }

        /// <summary>
        /// Gets the index whose value is set.
        /// </summary>
        [ProtoMember(1)]
        public Index Index { get; }

        /// <summary>
        /// Gets the item that is set.
        /// </summary>
        [ProtoMember(2)]
        public T Item { get; }

    }

}