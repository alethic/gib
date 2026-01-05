using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class SequenceClearSignal<T> : SequenceSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public SequenceClearSignal()
        {

        }

    }

}
