using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class SequenceAppendSignal<T> : SequenceSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public SequenceAppendSignal(T item)
        {
            Item = item;
        }

        /// <summary>
        /// Gets the item that is appended to the sequence.
        /// </summary>
        [ProtoMember(1)]
        public T Item { get; }

    }

}