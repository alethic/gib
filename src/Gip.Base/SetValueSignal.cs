using ProtoBuf;

namespace Gip.Base
{

    [ProtoContract]
    public class SetValueSignal<T> : ValueSignal<T>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="value"></param>
        public SetValueSignal(T value)
        {
            Value = value;
        }

        [ProtoMember(1)]
        public T Value { get; }

    }

}
