using Gip.Abstractions;

using ProtoBuf;

namespace Gip.Base.Collections
{

    [ProtoContract]
    [GenericProtoInclude(1, typeof(SequenceAppendSignal<>), [0])]
    [GenericProtoInclude(2, typeof(SequenceAppendManySignal<>), [0])]
    [GenericProtoInclude(3, typeof(SequenceClearSignal<>), [0])]
    [GenericProtoInclude(4, typeof(SequenceFreezeSignal<>), [0])]
    [GenericProtoInclude(5, typeof(SequenceResumeSignal<>), [0])]
    public abstract class SequenceSignal<T>
    {



    }

}
