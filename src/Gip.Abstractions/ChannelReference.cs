using System;
using System.Text.Json.Serialization;

using Gip.Abstractions.Json;

namespace Gip.Abstractions
{

    [JsonConverter(typeof(ChannelReferenceJsonConverter))]
    public readonly record struct ChannelReference(Uri Uri)
    {

        /// <summary>
        /// Arbitrary reference to an instance to keep it alive while it sits in the signal queue.
        /// </summary>
        public readonly object? Instance0 { get; init; }

    }

}
