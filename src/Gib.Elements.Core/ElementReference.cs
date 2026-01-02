using System;

using ProtoBuf;

namespace Gib.Core.Elements
{

    [ProtoContract]
    public readonly record struct ElementReference(
        [property: ProtoMember(1, Name = "uri")] Uri Uri
    )
    {

        public static implicit operator ElementReference(Uri elementUri) => new ElementReference(elementUri);

        public static implicit operator ElementReference(string elementUri) => new Uri(elementUri);

    }

}
