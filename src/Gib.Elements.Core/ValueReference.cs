using ProtoBuf;

namespace Gib.Core.Elements
{

    [ProtoContract]
    public readonly record struct ValueReference<T>(
        [property: ProtoMember(1, Name = "elementRef")] ElementReference ElementRef,
        [property: ProtoMember(2, Name = "propertyName")] string PropertyName)
    {



    }

}
