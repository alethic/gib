using System;

using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class ListRemoveManySignal<T> : ListSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="range"></param>
        public ListRemoveManySignal(Range range)
        {
            Range = range;
        }

        /// <summary>
        /// Gets the range of items to be removed.
        /// </summary>
        [ProtoMember(1)]
        public Range Range { get; }

    }

}