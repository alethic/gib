using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class ListClearSignal<T> : ListSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ListClearSignal()
        {

        }

    }

}