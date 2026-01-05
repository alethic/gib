using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class SetClearSignal<T> : SetSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public SetClearSignal()
        {

        }

    }

}
