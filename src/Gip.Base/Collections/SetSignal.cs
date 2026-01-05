using Gip.Abstractions;

using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    [GenericProtoInclude(1, typeof(SetAddSignal<>), [0])]
    [GenericProtoInclude(2, typeof(SetAddManySignal<>), [0])]
    [GenericProtoInclude(3, typeof(SetRemoveSignal<>), [0])]
    [GenericProtoInclude(4, typeof(SetRemoveManySignal<>), [0])]
    [GenericProtoInclude(5, typeof(SetClearSignal<>), [0])]
    [GenericProtoInclude(6, typeof(SetFreezeSignal<>), [0])]
    [GenericProtoInclude(7, typeof(SetResumeSignal<>), [0])]
    public abstract class SetSignal<T>
    {



    }

}
