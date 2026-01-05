using System;

using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class ListRemoveSignal<T> : ListSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="index"></param>
        public ListRemoveSignal(Index index)
        {
            Index = index;
        }

        /// <summary>
        /// Gets the index at which the item is removed.
        /// </summary>
        [ProtoMember(1)]
        public Index Index { get; }

    }

}