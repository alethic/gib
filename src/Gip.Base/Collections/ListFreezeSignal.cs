using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class ListFreezeSignal<T> : ListSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ListFreezeSignal()
        {

        }

    }

}