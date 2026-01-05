using System;
using System.Text.Json.Serialization;

using Gip.Abstractions.Json;

using ProtoBuf;

namespace Gip.Abstractions
{

    /// <summary>
    /// Signal structure that represents a reference to a function.
    /// </summary>
    /// <param name="Uri"></param>
    [JsonConverter(typeof(FunctionReferenceJsonConverter))]
    [ProtoContract]
    public readonly record struct FunctionReference(
        [property: ProtoMember(1)] Uri Uri)
    {

        /// <summary>
        /// Arbitrary reference to an instance to keep it alive while it sits in the signal queue.
        /// </summary>
        public readonly object? Instance0 { get; init; }

    }

}
