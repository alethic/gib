using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class SequenceResumeSignal<T> : SequenceSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public SequenceResumeSignal()
        {

        }

    }

}