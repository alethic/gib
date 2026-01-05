using Gip.Abstractions;

using ProtoBuf;

namespace Gip.Base
{

    [ProtoContract]
    [GenericProtoInclude(1, typeof(SetValueSignal<>), 0)]
    public abstract class ValueSignal<T>
    {

    }

}
