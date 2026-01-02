using System;

using ProtoBuf;

namespace Gib.Core.Elements
{

    [ProtoContract]
    public readonly record struct ElementTypeReference(
        [property: ProtoMember(1, Name = "uri")] Uri Uri
    );

}
