using System.Collections.Immutable;

using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class SequenceAppendManySignal<T> : SequenceSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="items"></param>
        public SequenceAppendManySignal(ImmutableArray<T> items)
        {
            Items = items;
        }

        /// <summary>
        /// Gets the set of items being appended.
        /// </summary>
        [ProtoMember(1)]
        public ImmutableArray<T> Items { get; }

    }

}