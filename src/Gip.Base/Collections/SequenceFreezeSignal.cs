using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class SequenceFreezeSignal<T> : SequenceSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public SequenceFreezeSignal()
        {

        }

    }

}