using Gip.Abstractions;

using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    [GenericProtoInclude(1, typeof(ListInsertSignal<>), [0])]
    [GenericProtoInclude(2, typeof(ListInsertManySignal<>), [0])]
    [GenericProtoInclude(3, typeof(ListRemoveSignal<>), [0])]
    [GenericProtoInclude(4, typeof(ListRemoveManySignal<>), [0])]
    [GenericProtoInclude(5, typeof(ListSetSignal<>), [0])]
    [GenericProtoInclude(6, typeof(ListSetManySignal<>), [0])]
    [GenericProtoInclude(7, typeof(ListClearSignal<>), [0])]
    [GenericProtoInclude(8, typeof(ListFreezeSignal<>), [0])]
    [GenericProtoInclude(9, typeof(ListResumeSignal<>), [0])]
    public abstract class ListSignal<T>
    {



    }

}
