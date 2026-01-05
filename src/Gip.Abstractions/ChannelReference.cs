using System;
using System.Text.Json.Serialization;

using Gip.Abstractions.Json;

using ProtoBuf;

namespace Gip.Abstractions
{

    [JsonConverter(typeof(ChannelReferenceJsonConverter))]
    [ProtoContract]
    public readonly record struct ChannelReference(
        [property: ProtoMember(1)] Uri Uri)
    {

        /// <summary>
        /// Arbitrary reference to an instance to keep it alive while it sits in the signal queue.
        /// </summary>
        public readonly object? Instance0 { get; init; }

    }

}
