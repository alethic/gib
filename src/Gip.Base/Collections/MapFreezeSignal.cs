using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    public class MapFreezeSignal<TKey, TValue> : MapSignal<TKey, TValue>
        where TKey : notnull
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MapFreezeSignal()
        {

        }

    }

}
