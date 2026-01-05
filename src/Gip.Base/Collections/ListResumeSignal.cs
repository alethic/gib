using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class ListResumeSignal<T> : ListSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ListResumeSignal()
        {

        }

    }

}